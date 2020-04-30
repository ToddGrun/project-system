﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.ProjectSystem.VS.Utilities;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Tree.Dependencies.AttachedCollections
{
    /// <summary>
    /// Base class for attached items in the dependencies tree in Solution Explorer.
    /// </summary>
    /// <remarks>
    /// Subclasses should probably derive from <see cref="RelatableItemBase"/>.
    /// </remarks>
    public abstract class AttachedCollectionItemBase
        : ITreeDisplayItem,
          ITreeDisplayItemWithImages,
          IPrioritizedComparable,
          IInteractionPatternProvider,
          IBrowsablePattern,
          INotifyPropertyChanged
    {
        // Other patterns we may wish to utilise in future are:
        //
        // - IInvocationPattern
        // - IContextMenuPattern
        // - ISupportExpansionEvents
        // - ISupportExpansionState
        // - IDragDropSourcePattern
        // - IDragDropTargetPattern
        // - ISupportDisposalNotification
        // - IRenamePattern
        // - IPivotItemProviderPattern
        // - We also see requests for IVsHierarchyItem
        //
        // NOTE we don't have to support ITreeDisplayItemWithImages -- it's covered by ITreeDisplayItem

        private static readonly HashSet<Type> s_supportedPatterns = new HashSet<Type>
        {
            typeof(ITreeDisplayItem),
            typeof(IBrowsablePattern)
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        private string _text;

        protected AttachedCollectionItemBase(string name)
        {
            Requires.NotNullOrWhiteSpace(name, nameof(name));

            _text = name;
        }

        public string Text
        {
            get => _text;
            protected set
            {
                if (_text != value)
                {
                    _text = value;
                    RaisePropertyChanged(KnownEventArgs.TextPropertyChanged);
                }
            }
        }

        public abstract int Priority { get; }

        public virtual FontStyle FontStyle => FontStyles.Normal;

        public virtual FontWeight FontWeight => FontWeights.Normal;

        public abstract ImageMoniker IconMoniker { get; }

        public virtual ImageMoniker ExpandedIconMoniker => IconMoniker;

        public virtual ImageMoniker OverlayIconMoniker => default;

        public virtual ImageMoniker StateIconMoniker => default;

        public virtual string? StateToolTipText => null;

        // Return null means ToolTipText is displayed only when the item's label is truncated
        public virtual object? ToolTipContent => null;

        public string ToolTipText => Text;

        public bool IsCut => false;

        public virtual object? GetBrowseObject() => null;

        TPattern IInteractionPatternProvider.GetPattern<TPattern>() where TPattern : class
        {
#pragma warning disable CS8603 // Possible null reference return. (https://github.com/dotnet/roslyn/issues/43619)
            if (s_supportedPatterns.Contains(typeof(TPattern)))
            {
                return this as TPattern;
            }

            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public virtual int CompareTo(object obj)
        {
            if (obj is ITreeDisplayItem item)
            {
                // Order by caption
                return StringComparer.OrdinalIgnoreCase.Compare(Text, item.Text);
            }

            return 0;
        }

        protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public override string ToString() => Text;
    }
}
