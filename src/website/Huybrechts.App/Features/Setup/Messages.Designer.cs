﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Huybrechts.App.Features.Setup {
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
    public class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Huybrechts.App.Features.Setup.Messages", typeof(Messages).Assembly);
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
        ///   Looks up a localized string similar to Duplicate country name for {0}.
        /// </summary>
        public static string DUPLICATE_SETUPCOUNTRY_CODE {
            get {
                return ResourceManager.GetString("DUPLICATE_SETUPCOUNTRY_CODE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate country code for {0}.
        /// </summary>
        public static string DUPLICATE_SETUPCOUNTRY_NAME {
            get {
                return ResourceManager.GetString("DUPLICATE_SETUPCOUNTRY_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate language for {0}.
        /// </summary>
        public static string DUPLICATE_SETUPLANGUAGE_NAME {
            get {
                return ResourceManager.GetString("DUPLICATE_SETUPLANGUAGE_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Duplicate system unit for {0}.
        /// </summary>
        public static string DUPLICATE_SETUPUNIT_NAME {
            get {
                return ResourceManager.GetString("DUPLICATE_SETUPUNIT_NAME", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find country with ID {0}.
        /// </summary>
        public static string INVALID_SETUPCOUNTRY_ID {
            get {
                return ResourceManager.GetString("INVALID_SETUPCOUNTRY_ID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find language with ID {0}.
        /// </summary>
        public static string INVALID_SETUPLANGUAGE_ID {
            get {
                return ResourceManager.GetString("INVALID_SETUPLANGUAGE_ID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find system unit with ID {0}.
        /// </summary>
        public static string INVALID_SETUPUNIT_ID {
            get {
                return ResourceManager.GetString("INVALID_SETUPUNIT_ID", resourceCulture);
            }
        }
    }
}
