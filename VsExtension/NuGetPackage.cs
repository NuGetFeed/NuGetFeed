namespace NuGetFeed.VSExtension {
    using System;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml.Linq;

    using EnvDTE;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", ProductVersion, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideBindingPath] // Definition dll needs to be on VS binding path
    [Guid(GuidList.guidNuGetPkgString)]
    public sealed class NuGetPackage : Package
    {
        // This product version will be updated by the build script to match the daily build version.
        // It is displayed in the Help - About box of Visual Studio
        public const string ProductVersion = "1.0.0.0";

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            AddMenuCommandHandlers();
        }

        private void AddMenuCommandHandlers()
        {
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // menu command for opening Manage NuGet packages dialog
                CommandID managePackageDialogCommandID = new CommandID(GuidList.guidNuGetDialogCmdSet, PkgCmdIDList.cmdidAddPackageDialog);
                OleMenuCommand managePackageDialogCommand = new OleMenuCommand(Execute, null, this.BeforeExecute, managePackageDialogCommandID);
                mcs.AddCommand(managePackageDialogCommand);
            }
        }

        public void VSProjectFileProps2()
        {
            var dte = (DTE)GetService(typeof(DTE));

            try
            {
                // Open a Visual C# or Visual Basic project
                // before running this add-in.
                Project project;
                ProjectItems projItems;
                ProjectItem projItem;
                Property prop;
                if (dte.SelectedItems.Count != 1)
                {
                    return;
                }

                var vsMonitorSelection = (IVsMonitorSelection)this.GetService(typeof(IVsMonitorSelection));
                IntPtr ppHier;
                uint pitemid;
                IVsMultiItemSelect ppMIS;
                IntPtr ppSC;
                vsMonitorSelection.GetCurrentSelection(out ppHier, out pitemid, out ppMIS, out ppSC);
                var o = (IVsHierarchy)Marshal.GetObjectForIUnknown(ppHier);
                Marshal.Release(ppHier);
                object pvar;
                if (o.GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out pvar) == VSConstants.S_OK)
                {
                    project = pvar as EnvDTE.Project;
                    if (project != null)
                    {
                        projItems = project.ProjectItems;
                        for (int i = 1; i <= projItems.Count; i++)
                        {
                            projItem = projItems.Item(i);
                            prop = projItem.Properties.Item("FileName");
                            if (prop == null || prop.Value == null)
                            {
                                continue;
                            }

                            if (!prop.Value.ToString().ToLower().Equals("packages.config"))
                            {
                                continue;
                            }

                            XDocument xml = XDocument.Load(projItem.Properties.Item("FullPath").Value);
                            var packages = xml.Descendants("package").Select(descendant => descendant.Attribute("id").Value).ToList();

                            if (packages.Count < 1)
                            {
                                continue;
                            }

                            var sb = new StringBuilder();
                            foreach (var s in packages)
                            {
                                sb.Append(s).Append(",");
                            }

                            var param = sb.ToString().TrimEnd(',');
                            System.Diagnostics.Process.Start("http://nugetfeed.org/List/Packages/" + param);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var eventSource = "NuGetFeed";
                if (!EventLog.SourceExists(eventSource))
                {
                    EventLog.CreateEventSource(eventSource, "Application");
                }

                EventLog.WriteEntry(eventSource, e.ToString());
            }
        }

        private uint GetItemId(object pvar)
        {
            if (pvar == null) return VSConstants.VSITEMID_NIL;
            if (pvar is int) return (uint)(int)pvar;
            if (pvar is uint) return (uint)pvar;
            if (pvar is short) return (uint)(short)pvar;
            if (pvar is ushort) return (uint)(ushort)pvar;
            if (pvar is long) return (uint)(long)pvar;
            return VSConstants.VSITEMID_NIL;
        }

        private void BeforeExecute(object sender, EventArgs args)
        {
        }

        private void Execute(object sender, EventArgs e)
        {
            VSProjectFileProps2();
        }
    }
}