﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Banananana
{
    public class WorkspaceData
    {
        public class Task
        {
            [JsonProperty]
            public String Text { get; set; }
        }

        public class Pile
        {
            [JsonProperty]
            public String Title { get; set; }

            [JsonProperty]
            public List<Task> Tasks { get; set; }

            public Pile()
            {
                Tasks = new List<Task>();
            }
        }
        
        public List<Pile> Piles { get; set; }


        public WorkspaceData()
        {
            Piles = new List<Pile>();
        }

        public void SafeToFile(String inFilename)
        {
            // Ensure path exists
            Directory.CreateDirectory(Path.GetDirectoryName(inFilename));

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamWriter sw = new StreamWriter(inFilename))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static String GetFlowDocumentContentsAsXML(FlowDocument inDocument)
        {
            using (var stream = new MemoryStream())
            {
                TextRange range = new TextRange(inDocument.ContentStart, inDocument.ContentEnd);
                range.Save(stream, DataFormats.Xaml);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
