﻿using System;
using System.Collections.Generic;
using MonoMod.ModInterop;

namespace NotSpikesmas
{
    internal static class CondensedSpoilerLogger
    {
        [ModImportName("CondensedSpoilerLogger")]
        private static class CondensedSpoilerLoggerImport
        {
            public static Action<string, Func<bool>, List<string>> AddCategorySafe = null;
        }
        static CondensedSpoilerLogger()
        {
            typeof(CondensedSpoilerLoggerImport).ModInterop();
        }
        /// <summary>
        /// Add a category to the condensed spoiler log.
        /// </summary>
        /// <param name="categoryName">The title to give the category.</param>
        /// <param name="test">Return false to skip adding this category to the log.</param>
        /// <param name="entries">A list of items to log in the category.</param>
        public static void AddCategorySafe(string categoryName, Func<bool> test, List<string> entries)
            => CondensedSpoilerLoggerImport.AddCategorySafe?.Invoke(categoryName, test, entries);
    }
}