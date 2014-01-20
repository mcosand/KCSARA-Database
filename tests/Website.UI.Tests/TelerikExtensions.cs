/*
 * Copyright 2012-2014 Matthew Cosand
 */
namespace Kcsara.Database.Website.Tests
{
    using System;
    using System.Threading;
    using ArtOfTest.WebAii.Controls;
    using ArtOfTest.WebAii.Core;
    using System.Collections.Generic;
    using System.Linq;

    public static class TelerikExtensions
    {
        public static T ByCustom<T>(this Find finder, Predicate<T> predicate, int milliseconds)
            where T : Control, new()
        {
            DateTime stop = DateTime.Now.AddMilliseconds(milliseconds);
            T result = null;
            do
            {
                result = finder.ByCustom<T>(predicate);
                if (result != null) break;

                Thread.Sleep(500);
                if (finder.AssociatedBrowser.FrameElement == null)
                {
                    finder.AssociatedBrowser.RefreshDomTree();
                }
                else
                {
                    finder.AssociatedBrowser.FrameElement.Parent.OwnerBrowser.Frames.RefreshAllDomTrees();
                }
            } while (DateTime.Now < stop);

            return result;
        }

        public static T ById<T>(this Find finder, string id, int milliseconds)
            where T : Control, new()
        {
            DateTime stop = DateTime.Now.AddMilliseconds(milliseconds);
            T result = null;
            do
            {
                result = finder.ById<T>(id);
                if (result != null) break;
                Thread.Sleep(500);
                if (finder.AssociatedBrowser.FrameElement == null)
                {
                    finder.AssociatedBrowser.RefreshDomTree();
                }
                else
                {
                    finder.AssociatedBrowser.FrameElement.Parent.OwnerBrowser.Frames.RefreshAllDomTrees();
                }
            } while (DateTime.Now < stop);

            return result;
        }

        public static T WaitForElement<T>(this Browser b, Func<Browser, IEnumerable<T>> elementGetter, Action<Browser> betweenSteps, int milliseconds)
            where T : Control, new()
        {
            DateTime stop = DateTime.Now.AddMilliseconds(milliseconds);
            T result = null;
            do
            {
                result = elementGetter(b).FirstOrDefault();
                if (result != null) break;

                Thread.Sleep(500);
                betweenSteps(b);
            } while (DateTime.Now < stop);

            if (result == null) throw new TimeoutException("Couldn't find element");

            return result;
        }

        public static void NavigateToPath(this Browser browser, string path)
        {
            Uri current = new Uri(browser.Url);
            browser.NavigateTo(new Uri(current, path));
        }
    }

}
