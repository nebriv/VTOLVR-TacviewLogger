using System.IO;
using System.Xml.Linq;
using UnityEngine;

namespace TacViewDataLogger
{
    public class heightmapGeneration
    {

        public support support = new support();
        public Texture2D gameHeightmap;

        // Source 
        // https://support.unity3d.com/hc/en-us/articles/206486626-How-can-I-get-pixels-from-unreadable-textures-?mobile_site=true
        private Texture2D getTexture(Texture2D unreadableTexture)
        {
            // Create a temporary RenderTexture of the same size as the texture
            RenderTexture tmp = RenderTexture.GetTemporary(
                                unreadableTexture.width,
                                unreadableTexture.height,
                                0,
                                RenderTextureFormat.Default,
                                RenderTextureReadWrite.Linear);

            // Blit the pixels on texture to the RenderTexture
            UnityEngine.Graphics.Blit(unreadableTexture, tmp);

            // Backup the currently set RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Set the current RenderTexture to the temporary one we created
            RenderTexture.active = tmp;

            // Create a new readable Texture2D to copy the pixels to it
            Texture2D myTexture2D = new Texture2D(unreadableTexture.width, unreadableTexture.height);


            // Copy the pixels from the RenderTexture to the new Texture
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();

            // Reset the active RenderTexture
            RenderTexture.active = previous;

            RenderTexture.ReleaseTemporary(tmp);

            return myTexture2D;
        }


        private Texture2D FlipTexture(Texture2D original, bool upSideDown = true)
        {

            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;


            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    if (upSideDown)
                    {
                        flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                    }
                    else
                    {
                        flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                    }
                }
            }
            flipped.Apply();

            return flipped;
        }

        private void saveHeightMap(Texture2D myTexture2D, VTMap map, string TacViewFolder)
        {

            support.WriteLog("Saving Height Map");

            myTexture2D = FlipTexture(myTexture2D, true);

            // Convert from RGBA32 to R16 - Shoutout to GentleLeviathan on Vtol VR Modding Discord!
            Texture2D toBecomeHeightmap = new Texture2D(myTexture2D.width, myTexture2D.height, TextureFormat.R16, false);

            Color[] newColors = new Color[myTexture2D.width * myTexture2D.height];

            for (int x = 0; x < toBecomeHeightmap.width; x++)
            {
                for (int y = 0; y < toBecomeHeightmap.height; y++)
                {
                    newColors[x + (y * toBecomeHeightmap.width)] = myTexture2D.GetPixel(x, y);
                }
            }

            toBecomeHeightmap.SetPixels(newColors);
            toBecomeHeightmap.Apply();

            byte[] rawBytes = toBecomeHeightmap.GetRawTextureData();

            string outFile = $"{TacViewFolder}heightmap_{genMapNameFile(map)}.data";

            support.WriteLog("Height Map Info:");
            support.WriteLog($"     Height: {toBecomeHeightmap.height} Width: {toBecomeHeightmap.width}");
            support.WriteLog($"     Latitude {map.mapLatitude}, Longitude {map.mapLongitude}");
            support.WriteLog($"     Map Size {map.mapSize}");
            support.WriteLog($"     Height Map Location: {outFile}");

            File.WriteAllBytes(outFile, rawBytes);

        }


        private geoHelper.GeoLocation[] generateCoords(VTMap map, int mapSize, bool startBottomLeft, float latOffset = 0, float lonOffset = 0)
        {
            support.WriteLog("Generating Coordinates from bottom corner");
            geoHelper.GeoLocation[] geoLocations = new geoHelper.GeoLocation[4];

            if (startBottomLeft)
            {
                geoHelper.GeoLocation bottomLeft = new geoHelper.GeoLocation();
                bottomLeft.Latitude = map.mapLatitude;
                bottomLeft.Longitude = map.mapLongitude;

                geoHelper.GeoLocation bottomRight = geoHelper.FindPointAtDistanceFrom2(bottomLeft, 90, mapSize);

                geoHelper.GeoLocation topLeft = geoHelper.FindPointAtDistanceFrom2(bottomLeft, 0, mapSize);

                geoHelper.GeoLocation topRight = geoHelper.FindPointAtDistanceFrom2(topLeft, 90, mapSize);

                geoLocations[0] = bottomLeft;
                geoLocations[1] = bottomRight;
                geoLocations[2] = topRight;
                geoLocations[3] = topLeft;

            }
            else
            {
                geoHelper.GeoLocation bottomRight = new geoHelper.GeoLocation();
                bottomRight.Latitude = map.mapLatitude;
                bottomRight.Longitude = map.mapLongitude;

                geoHelper.GeoLocation bottomLeft = geoHelper.FindPointAtDistanceFrom2(bottomRight, 270, mapSize);

                geoHelper.GeoLocation topLeft = geoHelper.FindPointAtDistanceFrom2(bottomLeft, 0, mapSize);

                geoHelper.GeoLocation topRight = geoHelper.FindPointAtDistanceFrom2(topLeft, 90, mapSize);
                geoLocations[0] = bottomLeft;
                geoLocations[1] = bottomRight;
                geoLocations[2] = topRight;
                geoLocations[3] = topLeft;
            }

            return geoLocations;
        }

        private string genMapNameFile(VTMap map)
        {
            string name;
            if (map.mapName == null)
            {
                name = "emptymapname";
            }
            else if (map.mapName == "")
            {
                name = "unknownmapname";
            }
            else
            {
                name = map.mapName;
            }

            return support.cleanString(name);
        }

        private void generateMapXML(Texture2D texture, VTMap map, bool customMap, string TacViewFolder)
        {

            support.WriteLog("Generating custom XML");

            geoHelper.GeoLocation[] geoLocations = new geoHelper.GeoLocation[4];

            if (customMap)
            {
                geoLocations = generateCoords(map, map.mapSize * 3, true);
            }
            else
            {
                geoHelper.GeoLocation bottomLeft = new geoHelper.GeoLocation
                {
                    Latitude = 53.94148616349887,
                    Longitude = -166.40063465537224
                };

                geoHelper.GeoLocation bottomRight = new geoHelper.GeoLocation
                {
                    Latitude = 53.94544,
                    Longitude = -165.426
                };

                geoHelper.GeoLocation topRight = new geoHelper.GeoLocation
                {
                    Latitude = 54.5124234999398,
                    Longitude = -165.41245674515972
                };

                geoHelper.GeoLocation topLeft = new geoHelper.GeoLocation
                {
                    Latitude = 54.51646078127723,
                    Longitude = -166.40063465537224
                };

                geoLocations[0] = bottomLeft;
                geoLocations[1] = bottomRight;
                geoLocations[2] = topRight;
                geoLocations[3] = topLeft;

            }

            string file = $"heightmap_{genMapNameFile(map)}.data";
            int endian = 1;
            int width = texture.width;
            int height = texture.height;

            float altFactor = 0.04f;
            float altOffset = -235f;

            string projection = "Quad";

            XDocument doc = new XDocument(new XElement("CustomHeightmap",
                                               new XElement("File", file),
                                               new XElement("BigEndian", endian.ToString()),
                                               new XElement("Width", width.ToString()),
                                               new XElement("Height", height.ToString()),
                                               new XElement("AltitudeFactor", altFactor.ToString()),
                                               new XElement("AltitudeOffset", altOffset.ToString()),
                                               new XElement("Projection", projection.ToString()),
                                               new XElement("BottomLeft",
                                                   new XElement("Longitude", geoLocations[0].Longitude),
                                                   new XElement("Latitude", geoLocations[0].Latitude)),
                                               new XElement("BottomRight",
                                                   new XElement("Longitude", geoLocations[1].Longitude),
                                                   new XElement("Latitude", geoLocations[1].Latitude)),
                                               new XElement("TopRight",
                                                   new XElement("Longitude", geoLocations[2].Longitude),
                                                   new XElement("Latitude", geoLocations[2].Latitude)),
                                               new XElement("TopLeft",
                                                   new XElement("Longitude", geoLocations[3].Longitude),
                                                   new XElement("Latitude", geoLocations[3].Latitude))
                                               ));

            support.WriteLog($"Saving custom tacview custom XML to {TacViewFolder + "customHeightMapXML.txt"}");
            doc.Save(TacViewFolder + "customHeightMapXML.txt");

        }

        public void getHeightMap(bool customScene, string TacViewFolder, VTMapManager mm)
        {

            if (!gameHeightmap)
            {
                // This probably shouldn't be hit anymore?
                if (customScene)
                {
                    support.WriteLog("Getting custom map");

                    VTMap map = support.mm.map;

                    if (map != null)
                    {
                        support.WriteLog("Getting texture data");
                        Texture2D myTexture2D = getTexture(support.mm.fallbackHeightmap);
                        saveHeightMap(myTexture2D, map, TacViewFolder);
                        generateMapXML(myTexture2D, map, customScene, TacViewFolder);
                    }
                    else
                    {
                        support.WriteLog("Unable to build heightmap. Map is null!");
                    }

                }
                else
                {
                    VTMap map = support.getMap();
                    support.WriteLog("Getting built in map");

                    Texture2D myTexture2D = getTexture(mm.fallbackHeightmap);
                    saveHeightMap(myTexture2D, map, TacViewFolder);
                    generateMapXML(myTexture2D, map, customScene, TacViewFolder);

                }
            }
            else
            {
                support.WriteLog("We already have a heightmap thank god.");
                VTMap map = support.mm.map;

                Texture2D myTexture2D = getTexture(gameHeightmap);
                saveHeightMap(myTexture2D, map, TacViewFolder);
                generateMapXML(myTexture2D, map, customScene, TacViewFolder);

            }

        }
    }
}
