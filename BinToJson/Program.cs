﻿using Spine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web.Script.Serialization;

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

            string skelFile = @"";
            string atlasFile = @"";

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
            // TODO 解析atlas文件
            return new Atlas(pages, regions);
        }


    }
    
    
}
