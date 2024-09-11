using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;

namespace BinToJson
{
    
    class Program
    {
        //Requires a skeleton file as input
        static void Main(string[] args) {
            //if (args.Length < 1) {
            //    Console.WriteLine("Usage: Drag a binary Spine skeleton file onto this executable and this outputs a json-ified version in the same directory");
            //    Console.ReadLine();
            //    return;
            //}
            //string fileName = args[0];

            string skelFile = @"test.skel";
            string atlasFile = @"test.atlas";

            SkeletonData skeletonData;

            //determines if the input file is json or bytes
            Atlas atlas = LoadAtlasData(atlasFile);
            if (skelFile.Contains("json")) {
                //Converting json -> json is unnecessary, but makes bug-checking significantly easier
                var sb = new SkeletonJson(atlas);
                skeletonData = sb.ReadSkeletonData(skelFile);
            } else {
                var sb = new SkeletonBinary(atlas);
                skeletonData = sb.ReadSkeletonData(skelFile);
            }
            //Takes the skeletonData and converts it into a serializable object
            Dictionary<string,object> jsonFile = SkelDataConverter.FromSkeletonData(skeletonData);

            //convert object to json string for storing
            JavaScriptSerializer jsonSerializer = new JavaScriptSerializer();
            jsonSerializer.MaxJsonLength = Int32.MaxValue;
            string json = jsonSerializer.Serialize(jsonFile);


            //Output file to same directory as input with "name 1", does not allow overwrites
            string preExtension = skelFile.Substring(0, skelFile.LastIndexOf('.'));
            int addNum = 1;
            string fullerName = preExtension;
            while(File.Exists(fullerName + ".json")) {
                fullerName = preExtension +" " + addNum;
                addNum++;
            }
            File.WriteAllText(fullerName+".json", json);
            

        }

        static Atlas LoadAtlasData(string atlasFile)
        {
            List<AtlasPage> pages = new List<AtlasPage>();
            List<AtlasRegion> regions = new List<AtlasRegion>();
            try
            {
                string[] lines = File.ReadAllLines(atlasFile);
                
                foreach (string line in lines)
                {
                    if(string.IsNullOrEmpty(line.Trim())) continue;
                    Console.WriteLine(line); // 打印每一行
                    if (line.StartsWith("SpineAG"))
                    {
                        pages.Add(new AtlasPage());
                        pages.Last().name = line;
                    }
                    else if (line.StartsWith("size:"))
                    {
                        pages.Last().width = int.Parse(line.Split(':')[1].Split(',')[0].Trim());
                        pages.Last().height = int.Parse(line.Split(':')[1].Split(',')[1].Trim());
                    }
                    else if (line.StartsWith("format:"))
                    {
                        Format? format = FindEnumByName<Format>(line);
                        pages.Last().format = (Format)format;
                    }
                    else if (line.StartsWith("filter:"))
                    {
                        TextureFilter? filter = FindEnumByName<TextureFilter>(line);
                        pages.Last().magFilter = (TextureFilter)filter;
                        pages.Last().minFilter = (TextureFilter)filter;
                    }
                    else if (Regex.Match(line, @"^[^:]+$").Success)
                    {
                        regions.Add(new AtlasRegion());
                        regions.Last().name = line;
                        regions.Last().page = pages.Last();
                    }
                    else if (line.StartsWith("  rotater:"))
                    {
                        bool rotate = line.Split(':')[1].Contains("true");
                        regions.Last().rotate = rotate;
                    }
                    else if (line.StartsWith("  xy:"))
                    {
                        regions.Last().x = int.Parse(line.Split(':')[1].Split(',')[0].Trim());
                        regions.Last().y = int.Parse(line.Split(':')[1].Split(',')[1].Trim());
                    }
                    else if (line.StartsWith("  size:"))
                    {
                        regions.Last().width = int.Parse(line.Split(':')[1].Split(',')[0].Trim());
                        regions.Last().height = int.Parse(line.Split(':')[1].Split(',')[1].Trim());
                    }
                    else if (line.StartsWith("  orig:"))
                    {
                        regions.Last().originalWidth = int.Parse(line.Split(':')[1].Split(',')[0].Trim());
                        regions.Last().originalHeight = int.Parse(line.Split(':')[1].Split(',')[1].Trim());
                    }
                    else if (line.StartsWith("  offset:"))
                    {
                        regions.Last().offsetX = float.Parse(line.Split(':')[1].Split(',')[0].Trim());
                        regions.Last().offsetY = float.Parse(line.Split(':')[1].Split(',')[1].Trim());
                    }
                    else if (line.StartsWith("  index:"))
                    {
                        regions.Last().index = int.Parse(line.Split(':')[1].Trim());
                    }


                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("指定的文件未找到。");
            }
            catch (IOException ex)
            {
                Console.WriteLine("读取文件时发生错误：" + ex.Message);
            }

            return new Atlas(pages, regions);
        }

        static T? FindEnumByName<T>(string input) where T : struct, Enum
        {
            Type enumType = typeof(T);
            foreach (T value in Enum.GetValues(enumType))
            {
                string enumName = Enum.GetName(enumType, value);
                if (input.Contains(enumName))
                {
                    return value;
                }
            }
            return null;
        }


    }
    
    
}
