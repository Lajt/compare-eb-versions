﻿#pragma checksum "..\..\..\..\OldPlayerMover\Gui.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "56367F31219C1ED072DF01C9D5C699EA8B8B6AAD"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Legacy.OldPlayerMover;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Legacy.OldPlayerMover {
    
    
    /// <summary>
    /// Gui
    /// </summary>
    public partial class Gui : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 35 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxAvoidWallHugging;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxDebugInputApi;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxUseMouseSmoothing;
        
        #line default
        #line hidden
        
        
        #line 57 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxDebugAdjustments;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox CheckBoxForceAdjustCombatAreas;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\..\OldPlayerMover\Gui.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid DataGridForcedAdjustmentAreas;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Legacy;component/oldplayermover/gui.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\OldPlayerMover\Gui.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.CheckBoxAvoidWallHugging = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 2:
            this.CheckBoxDebugInputApi = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 3:
            this.CheckBoxUseMouseSmoothing = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 4:
            this.CheckBoxDebugAdjustments = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 5:
            this.CheckBoxForceAdjustCombatAreas = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 6:
            this.DataGridForcedAdjustmentAreas = ((System.Windows.Controls.DataGrid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

