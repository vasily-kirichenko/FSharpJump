using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Windows;
using Microsoft.FSharp.Core;

namespace FSharpEditorEnhancements
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("F#")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class FSharpJumpFilterProvider : IVsTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("FSharpJump")]
        [Order(After = PredefinedAdornmentLayers.Text)]
        [TextViewRole(PredefinedTextViewRoles.Interactive)]
        internal AdornmentLayerDefinition fsharpJumpAdornmentLayer;


        [Import(typeof(IVsEditorAdaptersFactoryService))]
        internal IVsEditorAdaptersFactoryService editorFactory = null;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = editorFactory.GetWpfTextView(textViewAdapter);
            if (textView == null)
                return;
            Converter<IWpfTextView,UIElement> f = new Converter<IWpfTextView,UIElement>((v) => new FSharpJumpList(v));
            FSharpFunc<IWpfTextView, UIElement> ff = FSharpFunc<IWpfTextView, UIElement>.FromConverter(f);
            AddCommandFilter(textViewAdapter, new FSharpJumpCore.FSharpJumpCommandFilter(textView, ff));
        }

        void AddCommandFilter(IVsTextView viewAdapter, FSharpJumpCore.FSharpJumpCommandFilter commandFilter)
        {
            if (commandFilter.IsAdded == false)
            {
                //get the view adapter from the editor factory
                IOleCommandTarget next;
                int hr = viewAdapter.AddCommandFilter(commandFilter, out next);

                if (hr == VSConstants.S_OK)
                {
                    commandFilter.IsAdded = true;
                    //you'll need the next target for Exec and QueryStatus
                    if (next != null)
                        commandFilter.NextTarget = next;
                }
            }
        }
    }
}
