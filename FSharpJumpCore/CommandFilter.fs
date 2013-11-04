namespace FSharpJumpCore
open System
open System.Runtime.InteropServices
open Microsoft.VisualStudio.OLE.Interop
open Microsoft.VisualStudio
open Microsoft.VisualStudio.Text.Editor
open System.Diagnostics
open System.Windows
open System.Windows.Controls

module FSharpJumpConstants =
    let adornmentLayerName = "FSharpJump"

type FSharpJumpCommandFilter(textView:IWpfTextView, adornmentFactory:IWpfTextView->UIElement) =

    let mutable nextTarget:IOleCommandTarget = null
    let mutable isAdded = false

    member x.NextTarget with get() = nextTarget and set(v) = nextTarget <- v
    member x.IsAdded with get() = isAdded and set(v) = isAdded <- v

    interface IOleCommandTarget with
        member x.QueryStatus(pguidCmdGroup,cCmds,prgCmds,pCmdText) = 
            x.NextTarget.QueryStatus(ref pguidCmdGroup,cCmds,prgCmds,pCmdText)
        member x.Exec(pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut) =
            if pguidCmdGroup = VSConstants.VSStd2K && nCmdID = 147u then
                let adornment = adornmentFactory(textView)
                Canvas.SetTop(adornment, textView.Caret.ContainingTextViewLine.Top)
                Canvas.SetLeft(adornment, textView.Caret.Right)
                let layer = textView.GetAdornmentLayer(FSharpJumpConstants.adornmentLayerName);
                layer.RemoveAllAdornments()
                layer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, new Nullable<Text.SnapshotSpan>(), null, adornment, null) |> ignore
            x.NextTarget.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut)
