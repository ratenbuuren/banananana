﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Banananana
{

    /// <summary>
    /// Workspace containing all data, serialized/deserialized to/from disk (in json format)
    /// </summary>
    public class Workspace
    {
        /// <summary>
        /// A category for tasks
        /// </summary>
        public class Category
        {
            private string mTitle;
            private Color mColor;

            [JsonProperty]
            public String Title 
            {
                get { return mTitle; }
                set { Instance.MarkAsDirty(); mTitle = value; } 
            }

            [JsonProperty]
            public System.Windows.Media.Color Color
            {
                get { return mColor; }
                set { Instance.MarkAsDirty(); mColor = value; }
            }

            public Category()
            {
                mTitle = "";
                mColor = System.Windows.Media.Color.FromArgb(255, 238, 173, 0);
            }
        }


        /// <summary>
        /// A link to an external site
        /// </summary>
        public class ExternalLink
        {
            private string mTarget;

            [JsonProperty]
            public String Target
            {
                get { return mTarget; }
                set { Instance.MarkAsDirty(); mTarget = value; }
            }

            public ExternalLink()
            {
                mTarget = "https://www.google.com";
            }
        }

        /// <summary>
        /// A single task, can contain external links and notes
        /// </summary>
        public class Task
        {
            public const String cNewNotesText = "<Section xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" xml:space=\"preserve\" TextAlignment=\"Left\" LineHeight=\"Auto\" IsHyphenationEnabled=\"False\" xml:lang=\"en-us\" FlowDirection=\"LeftToRight\" NumberSubstitution.CultureSource=\"Text\" NumberSubstitution.Substitution=\"AsCulture\" FontFamily=\"/Banananana;component/Resources/#Nunito\" FontStyle=\"Normal\" FontWeight=\"Normal\" FontStretch=\"Normal\" FontSize=\"12\" Foreground=\"#FF000000\" Typography.StandardLigatures=\"True\" Typography.ContextualLigatures=\"True\" Typography.DiscretionaryLigatures=\"False\" Typography.HistoricalLigatures=\"False\" Typography.AnnotationAlternates=\"0\" Typography.ContextualAlternates=\"True\" Typography.HistoricalForms=\"False\" Typography.Kerning=\"True\" Typography.CapitalSpacing=\"False\" Typography.CaseSensitiveForms=\"False\" Typography.StylisticSet1=\"False\" Typography.StylisticSet2=\"False\" Typography.StylisticSet3=\"False\" Typography.StylisticSet4=\"False\" Typography.StylisticSet5=\"False\" Typography.StylisticSet6=\"False\" Typography.StylisticSet7=\"False\" Typography.StylisticSet8=\"False\" Typography.StylisticSet9=\"False\" Typography.StylisticSet10=\"False\" Typography.StylisticSet11=\"False\" Typography.StylisticSet12=\"False\" Typography.StylisticSet13=\"False\" Typography.StylisticSet14=\"False\" Typography.StylisticSet15=\"False\" Typography.StylisticSet16=\"False\" Typography.StylisticSet17=\"False\" Typography.StylisticSet18=\"False\" Typography.StylisticSet19=\"False\" Typography.StylisticSet20=\"False\" Typography.Fraction=\"Normal\" Typography.SlashedZero=\"False\" Typography.MathematicalGreek=\"False\" Typography.EastAsianExpertForms=\"False\" Typography.Variants=\"Normal\" Typography.Capitals=\"Normal\" Typography.NumeralStyle=\"Normal\" Typography.NumeralAlignment=\"Normal\" Typography.EastAsianWidths=\"Normal\" Typography.EastAsianLanguage=\"Normal\" Typography.StandardSwashes=\"0\" Typography.ContextualSwashes=\"0\" Typography.StylisticAlternates=\"0\"><Paragraph><Run></Run></Paragraph></Section>";

            private string mText;
            private string mNotes;
            private int mCategoryIndex;
            private List<ExternalLink> mExternalLinks;

            [JsonProperty]
            public String Text
            {
                get { return mText; }
                set { Instance.MarkAsDirty(); mText = value; }
            }

            [JsonProperty]
            public String Notes
            {
                get { return mNotes; }
                set { Instance.MarkAsDirty(); mNotes = value; }
            }

            [JsonProperty]
            public int CategoryIndex
            {
                get { return mCategoryIndex; }
                set { Instance.MarkAsDirty(); mCategoryIndex = value; }
            }

            [JsonProperty]
            public List<ExternalLink> ExternalLinks
            {
                get { return mExternalLinks; }
                set { mExternalLinks = value; }
            }

            public Task()
            {
                mExternalLinks = new List<ExternalLink>();
                mCategoryIndex = -1;
            }

            public void AddExternalLink(ExternalLink inLink)
            {
                mExternalLinks.Add(inLink);
                Instance.MarkAsDirty();
            }

            public void RemoveExternalLink(ExternalLink inLink)
            {
                mExternalLinks.Remove(inLink);
                Instance.MarkAsDirty();
            }
        }

        /// <summary>
        /// A pile of tasks, with a title
        /// </summary>
        public class Pile
        {
            private string mTitle;
            private Color mColor;
            private List<Task> mTasks;


            [JsonProperty]
            public String Title
            {
                get { return mTitle; }
                set { Instance.MarkAsDirty(); mTitle = value; }
            }

            [JsonProperty]
            public System.Windows.Media.Color Color
            {
                get { return mColor; }
                set { Instance.MarkAsDirty(); mColor = value; }
            }

            [JsonProperty]
            public List<Task> Tasks
            {
                get { return mTasks; }
                set { mTasks = value; }
            }

            public Pile()
            {
                Tasks = new List<Task>();
                mColor = System.Windows.Media.Color.FromArgb(255,238,173,0);
            }

            public void InsertTask(int inIndex, Task inTask)
            {
                mTasks.Insert(inIndex, inTask);
                Instance.MarkAsDirty();
            }

            public void RemoveTask(Task inTask)
            {
                mTasks.Remove(inTask);
                Instance.MarkAsDirty();
            }
        }

        private static Mutex sSaveMutex = new Mutex();
        private static Workspace sInstance = null;

        public static Workspace Instance
        {
            get
            {
                if (sInstance == null)
                    sInstance = new Workspace();
                return sInstance;
            }
            set { sInstance = value; }
        }
 
        public bool IsDirty { get; private set; }

        /// <summary>
        /// All the task piles that the user has defined
        /// </summary>
        public List<Pile> Piles { get; set; }

        public void AddPile(Pile inPile)
        {
            Piles.Add(inPile);
            Instance.MarkAsDirty();
        }

        public void RemovePile(Pile inPile)
        {
            Piles.Remove(inPile);
            Instance.MarkAsDirty();
        }

        public void InsertPile(int inIndex, Pile inPile)
        {
            Piles.Insert(inIndex, inPile);
            Instance.MarkAsDirty();
        }

        public void RemovePileAt(int inIndex)
        {
            Piles.RemoveAt(inIndex);
            Instance.MarkAsDirty();
        }


        /// <summary>
        /// All the task categories that the user has defined
        /// </summary>
        public List<Category> Categories { get; set; }

        public void AddCategory(Category inCategory)
        {
            Categories.Add(inCategory);
            Instance.MarkAsDirty();
        }

        public void RemoveCategoryAt(int inIndex)
        {
            Categories.RemoveAt(inIndex);
            Instance.MarkAsDirty();
        }


        private Workspace()
        {
            IsDirty = false;
            Piles = new List<Pile>();
            Categories = new List<Category>();
        }

        public void MarkAsDirty()
        {
            IsDirty = true;
        }

        /// <summary>
        /// Serialize our complete workspace to disk
        /// </summary>
        /// <param name="inFilename"></param>
        public void SaveToFile(String inFilename)
        {
            sSaveMutex.WaitOne();
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
            Instance.IsDirty = false;
            sSaveMutex.ReleaseMutex();
        }

        /// <summary>
        /// Load a workspace from disk
        /// </summary>
        /// <param name="inFilename"></param>
        /// <returns></returns>
        public static void LoadFromFile(String inFilename)
        {
            sSaveMutex.WaitOne();
            Workspace data = Instance;

            if (!File.Exists(inFilename))
                return;

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;

            using (StreamReader sr = new StreamReader(inFilename))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                data = serializer.Deserialize<Workspace>(reader);
            }

            Instance = data;
            Instance.IsDirty = false;
            sSaveMutex.ReleaseMutex();
        }

        /// <summary>
        /// Utility function to extract content from a FlowDocument object as a single string of XML data (utf-8 encoded)
        /// </summary>
        /// <param name="inDocument"></param>
        /// <returns></returns>
        public static String GetFlowDocumentContentAsXML(FlowDocument inDocument)
        {
            using (var stream = new MemoryStream())
            {
                TextRange range = new TextRange(inDocument.ContentStart, inDocument.ContentEnd);
                range.Save(stream, DataFormats.Xaml);

                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// Utility function to load the content of a FlowDocument object from a single string of XML data (utf-8 encoded)
        /// </summary>
        /// <param name="inDocument"></param>
        /// <param name="inTextXML"></param>
        public static void SetFlowDocumentContentFromXML(FlowDocument inDocument, String inTextXML)
        {            
            byte[] data = Encoding.UTF8.GetBytes(inTextXML);
            using (var stream = new MemoryStream(data))
            {
                TextRange range = new TextRange(inDocument.ContentStart, inDocument.ContentEnd);
                range.Load(stream, DataFormats.Xaml);
            }                       
        }
    }
}
