﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Hexpoint.Blox.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Hexpoint.Blox.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;xsd:schema xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///	&lt;xsd:element name=&quot;Config&quot;&gt;
        ///		&lt;xsd:complexType&gt;
        ///			&lt;xsd:all&gt;
        ///				&lt;xsd:element name=&quot;Mode&quot; minOccurs=&quot;0&quot; maxOccurs=&quot;1&quot;&gt;
        ///					&lt;xsd:simpleType&gt;
        ///						&lt;xsd:restriction base=&quot;xsd:string&quot;&gt;
        ///							&lt;xsd:minLength value=&quot;0&quot; /&gt;
        ///							&lt;xsd:maxLength value=&quot;32&quot; /&gt;
        ///						&lt;/xsd:restriction&gt;
        ///					&lt;/xsd:simpleType&gt;
        ///				&lt;/xsd:element&gt;
        ///				&lt;xsd:element name=&quot;UserName&quot; minOccurs=&quot;0&quot; maxOccurs=&quot;1&quot;&gt;
        ///					&lt;xsd:simpleType&gt;
        ///						&lt;xsd: [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Config {
            get {
                return ResourceManager.GetString("Config", resourceCulture);
            }
        }
        
        internal static System.Drawing.Bitmap GreenCheck {
            get {
                object obj = ResourceManager.GetObject("GreenCheck", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap GreenFlag {
            get {
                object obj = ResourceManager.GetObject("GreenFlag", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap LauncherBackground2 {
            get {
                object obj = ResourceManager.GetObject("LauncherBackground2", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        internal static System.Drawing.Bitmap Progress {
            get {
                object obj = ResourceManager.GetObject("Progress", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
