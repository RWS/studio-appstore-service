﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppStoreIntegrationServiceCore {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class ServiceResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ServiceResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AppStoreIntegrationServiceCore.ServiceResource", typeof(ServiceResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_BLOBNAME.
        /// </summary>
        public static string BlobName {
            get {
                return ResourceManager.GetString("BlobName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_COMMENTSFILENAME.
        /// </summary>
        public static string CommentsFileName {
            get {
                return ResourceManager.GetString("CommentsFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;body style=&quot;border: 1px solid lightgray; padding:20px; width: 500px&quot;&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;b&gt;{0}&lt;/b&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;b&gt;&lt;br&gt;&lt;/b&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;img src=&quot;{1}&quot; width=&quot;250&quot;&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;br&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;b&gt;{2}&lt;/b&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;b&gt;&lt;br&gt;&lt;/b&gt;&lt;/div&gt;
        ///&lt;div style=&quot;text-align: center;&quot;&gt;&lt;a href=&quot;{3}&quot;&gt;View&lt;/a&gt;&lt;b&gt;&lt;br&gt;&lt;/b&gt;&lt;/div&gt;
        ///&lt;/body&gt;.
        /// </summary>
        public static string EmailNotificationTemplate {
            get {
                return ResourceManager.GetString("EmailNotificationTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] FontNames {
            get {
                object obj = ResourceManager.GetObject("FontNames", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_LOCAL_FOLDERPATH.
        /// </summary>
        public static string LocalFolderPath {
            get {
                return ResourceManager.GetString("LocalFolderPath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_LOGSFILENAME.
        /// </summary>
        public static string LogsFileName {
            get {
                return ResourceManager.GetString("LogsFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_MAPPINGFILENAME.
        /// </summary>
        public static string MappingFileName {
            get {
                return ResourceManager.GetString("MappingFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_PLUGINSFILENAME.
        /// </summary>
        public static string PluginsFileName {
            get {
                return ResourceManager.GetString("PluginsFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;div class=&quot;d-flex p-3 border-bottom&quot; style=&quot;width:400px;&quot;&gt;
        ///&lt;div class=&quot;d-flex flex-column pe-3 border-end&quot;&gt;
        ///&lt;img src=&quot;{1}&quot; width=&quot;50&quot;&gt;
        ///&lt;p class=&quot;m-0 text-center fw-bold&quot; style=&quot;font-size:10px;&quot;&gt;{2}&lt;/p&gt;
        ///&lt;/div&gt;
        ///&lt;div class=&quot;w-100 ps-3&quot;&gt;
        ///&lt;p class=&quot;m-0&quot;&gt;{0}&lt;/p&gt;
        ///&lt;a class=&quot;text-decoration-none&quot; href=&quot;{3}&quot;&gt;View&lt;/a&gt;
        ///&lt;p class=&quot;m-0 text-end&quot; style=&quot;font-size:12px;&quot;&gt;{4}&lt;/p&gt;
        ///&lt;/div&gt;
        ///&lt;/div&gt;.
        /// </summary>
        public static string PushNotificationTemplate {
            get {
                return ResourceManager.GetString("PushNotificationTemplate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_SETTINGSFILENAME.
        /// </summary>
        public static string SettingsFileName {
            get {
                return ResourceManager.GetString("SettingsFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_STORAGE_ACCOUNTKEY.
        /// </summary>
        public static string StorageAccountKey {
            get {
                return ResourceManager.GetString("StorageAccountKey", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPSTOREINTEGRATION_STORAGE_ACCOUNTNAME.
        /// </summary>
        public static string StorageAccountName {
            get {
                return ResourceManager.GetString("StorageAccountName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to APPINSIGHTS_INSTRUMENTATIONKEY.
        /// </summary>
        public static string TelemetryInstrumentationKey {
            get {
                return ResourceManager.GetString("TelemetryInstrumentationKey", resourceCulture);
            }
        }
    }
}
