// -----------------------------------------------------------------------
// <copyright file="FlowDocumentSourceBehavior.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI.Behaviors
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;

    /// <summary>
    /// This class represents FlowDocumentSourceBehavior class.
    /// </summary>
    public static class FlowDocumentSourceBehavior
    {
        public static readonly DependencyProperty DocumentSourceProperty =
            DependencyProperty.RegisterAttached(
            "DocumentSource",
            typeof(object),
            typeof(FlowDocumentSourceBehavior),
            new PropertyMetadata(FlowDocumentSourceChanged)
            );

        private static void FlowDocumentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var flowDocumentScrollViewer = d as FlowDocumentScrollViewer;
            if (flowDocumentScrollViewer != null)
            {
                flowDocumentScrollViewer.Document = (FlowDocument)e.NewValue;
            }
        }

        public static void SetDocumentSource(DependencyObject target, object value)
        {
            target.SetValue(DocumentSourceProperty, value);
        }

        public static object GetDocumentSource(DependencyObject target)
        {
            return target.GetValue(DocumentSourceProperty);
        }

    }
}
