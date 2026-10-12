using System.Globalization;
using System.Linq;
using ClickCook.Data;

namespace ClickCook.UI
{
    /// <summary>Presentation-only Ukrainian copy. Authored IDs and design values are unchanged.</summary>
    public static class ClickCookUiCopy
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("uk-UA");
        private static readonly string[] Locations =
        {
            "Гуртожиток", "Кафе", "Закусочна", "Фестивальний фудкорт", "Гриль-ресторан",
            "Домашній ресторан", "Італійський ресторан", "Кондитерська", "Японський ресторан",
            "Модний ресторан", "Власний мішленівський ресторан"
        };
        private static readonly string[] Dishes =
        {
            "Сендвічі", "Кекси у чашці", "Французький хот-дог", "Пончики", "Картопля фрі",
            "Бургери", "Овочевий салат", "Кебаб", "Овочі на грилі", "М’ясо на грилі",
            "Риба на грилі", "Паста болоньєзе", "Піца", "Пиріг", "Джелато", "Тірамісу",
            "Ромова баба", "Мусові тістечка", "Макарони", "Круасан", "Преміальний рамен",
            "Моті", "Тартар", "Морські гребінці", "Яловичина Веллінгтон", "Грибне ризото",
            "Качка конфі", "Восьминіг", "Суші з вугром", "Лобстер термідор", "Ікра з млинцями",
            "Імператорський дегустаційний сет"
        };
        public static string Location(LocationData location) => location != null && location.LevelRequirement >= 0 &&
            location.LevelRequirement < Locations.Length ? Locations[location.LevelRequirement] : location?.EnglishName ?? string.Empty;
        public static string Dish(DishData dish) => dish != null && dish.DishId >= 1 && dish.DishId <= Dishes.Length
            ? Dishes[dish.DishId - 1] : dish?.EnglishName ?? "Немає даних";
        public static string Money(decimal value) => value.ToString("N2", Culture) + " у.е.";
        public static string Rarity(ClickCook.Runtime.DishRarity rarity) => rarity switch
        {
            ClickCook.Runtime.DishRarity.Common => "Звичайна",
            ClickCook.Runtime.DishRarity.Uncommon => "Незвичайна",
            ClickCook.Runtime.DishRarity.Rare => "Рідкісна",
            ClickCook.Runtime.DishRarity.Fantastic => "Фантастична",
            ClickCook.Runtime.DishRarity.Legendary => "Легендарна",
            _ => "Невідома"
        };
        public static string DishPreview(LocationData location)
        {
            var dishes = location.DocumentedAvailableDishes;
            if (dishes.Count == 0) return "У цій локації немає доступних замовлень";
            return $"Страви локації: {dishes.Count}\n" + string.Join(" · ", dishes.Take(3).Select(Dish)) +
                (dishes.Count > 3 ? $"\nта ще {dishes.Count - 3}" : string.Empty);
        }
    }
}
