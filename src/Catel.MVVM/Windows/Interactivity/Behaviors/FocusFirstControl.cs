﻿namespace Catel.Windows.Interactivity
{
    using System;
    using System.Windows;

    /// <summary>
    /// Behavior to focus the first control in a window.
    /// </summary>
    public class FocusFirstControl : BehaviorBase<FrameworkElement>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the parent should be focused first. 
        /// <para />
        /// The default value is <c>true</c>.
        /// </summary>
        /// <value><c>true</c> if the parent should be focused first; otherwise, <c>false</c>.</value>
        public bool FocusParentsFirst
        {
            get { return (bool) GetValue(FocusParentsFirstProperty); }
            set { SetValue(FocusParentsFirstProperty, value); }
        }

        /// <summary>
        /// Dependency property for the <see cref="FocusParentsFirst"/> property.
        /// </summary>
        public static readonly DependencyProperty FocusParentsFirstProperty = DependencyProperty.Register(nameof(FocusParentsFirst),
            typeof(bool), typeof(FocusFirstControl), new PropertyMetadata(true));

        /// <summary>
        /// Called when the associated object is loaded.
        /// </summary>
        protected override void OnAssociatedObjectLoaded()
        {
            base.OnAssociatedObjectLoaded();

            AssociatedObject.FocusFirstControl(FocusParentsFirst);
        }
    }
}
