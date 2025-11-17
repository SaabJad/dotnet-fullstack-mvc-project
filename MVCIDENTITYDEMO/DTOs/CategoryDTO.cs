namespace MVCIDENTITYDEMO.DTOs
{
    public class CategoryDTO  
        //kermel circular dependeny, for security 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
}