using System;
using System.Collections.Generic;

namespace Dinah.Core.EnumerationExamples
{
    // with extension methods and mapping table
    public enum TagEnum { Abridged = 1, Gift = 3 }
    public static class Tag_Enum_2_Extensions
    {
        private static Dictionary<TagEnum, bool> isUserAssignable_Dic { get; } = new Dictionary<TagEnum, bool> { [TagEnum.Abridged] = false, [TagEnum.Gift] = true };
        public static bool IsUserAssignable(this TagEnum tag_Enum) => isUserAssignable_Dic[tag_Enum];
    }

    // without behavior (ie: if only values), this is just a fancy way to avoiding a mapping dictionary or data table.
    // actually, mapping dictionary can also map to functions

    sealed class PropertyMapping : Enumeration
    {
        public static readonly PropertyMapping Manager = new PropertyMapping(0, "Ümlaut Über Manager", 99, () => Console.WriteLine("MANAGER POWER"));
        public static readonly PropertyMapping Servant = new PropertyMapping(1, "Simpleton", -1, () => throw new Exception("not cool"));

        public int Bonus { get; }

        private Action action { get; }
        public void DoMyAction() => action();

        private PropertyMapping(int value, string displayName, int bonus, Action myAction) : base(value, displayName)
        {
            Bonus = bonus;
            action = myAction;
        }
    }

    public abstract class SubClassing : Enumeration<SubClassing>
    {
        //
        // these may be fields or properties
        //
        public static readonly SubClassing Manager = new ManagerType();
        public static SubClassing Servant { get; } = new ServantType();

        private SubClassing(int value, string displayName) : base(value, displayName) { }

        public abstract decimal BonusSize { get; }

        private class ManagerType : SubClassing
        {
            public ManagerType() : base(0, "Manager") { }
            public override decimal BonusSize => 1000m;
        }

        private class ServantType : SubClassing
        {
            public ServantType() : base(1, "Servant") { }
            public override decimal BonusSize => 0m;
        }
    }

    // nesting primative inside Enumeration<>
    public enum Page_PrimitiveEnum { Library = 1, Series = 2, Product = 3 } // can expand. eg: authors, search results, featured
    public abstract class PageEnum : Enumeration<PageEnum>
    {
        public static PageEnum Library { get; } = new LibraryPage();
        public static PageEnum Product { get; } = new ProductPage();

        protected PageEnum(int value, string displayName) : base(value, displayName) { }

        private class LibraryPage : PageEnum { public LibraryPage() : base((int)Page_PrimitiveEnum.Library, "Library") { } }
        private class ProductPage : PageEnum { public ProductPage() : base((int)Page_PrimitiveEnum.Product, "Product") { } }
    }

    // nesting Enumeration<> inside Enumeration<>
    // this is composition, NOT inheritance. more code needed but far more versatile. including using singletons for WebPageEnum or PageEnum
    public abstract class WebPageEnum : Enumeration<WebPageEnum>
    {
        public static WebPageEnum Library { get; } = new LibraryWebPage();
        public static WebPageEnum Product { get; } = new ProducttWebPage();

        public PageEnum BackingPageEnum => PageEnum.FromValue(Value);

        protected WebPageEnum(PageEnum basePage) : base(basePage.Value, basePage.DisplayName) { }

        public static WebPageEnum FromValue(PageEnum pageEnum) => FromValue(pageEnum.Value);

        private class LibraryWebPage : WebPageEnum { public LibraryWebPage() : base(PageEnum.Library) { } }
        private class ProducttWebPage : WebPageEnum { public ProducttWebPage() : base(PageEnum.Product) { } }
    }
}
