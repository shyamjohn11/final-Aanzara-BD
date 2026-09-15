namespace ECommercePlatform.Domain.Constants;

public static class Permissions
{
    public static class Category
    {
        public const string View = "Category.View";
        public const string Create = "Category.Create";
        public const string Update = "Category.Update";
        public const string Delete = "Category.Delete";
    }

    public static class Product
    {
        public const string View = "Product.View";
        public const string Create = "Product.Create";
        public const string Update = "Product.Update";
        public const string Delete = "Product.Delete";
    }

    public static class Brand
    {
        public const string View = "Brand.View";
        public const string Create = "Brand.Create";
        public const string Update = "Brand.Update";
        public const string Delete = "Brand.Delete";
    }

    public static class Warehouse
    {
        public const string View = "Warehouse.View";
        public const string Create = "Warehouse.Create";
        public const string Update = "Warehouse.Update";
        public const string Delete = "Warehouse.Delete";
    }

    public static class Inventory
    {
        public const string View = "Inventory.View";
        public const string Add = "Inventory.Add";
        public const string Adjust = "Inventory.Adjust";
    }
}