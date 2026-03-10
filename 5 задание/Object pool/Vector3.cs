namespace ObjectPoolBulletExample
{
    // Простая структура с двумя координатами, чтобы не тянуть
    // тяжелые зависимости. Вроде бы "Vector3", но на деле 2D.
    // Используется для хранения позиции и вычислений в примерной модели.
    // Структура для представления позиции в 2D пространстве
    public struct Vector3
    {
        public float x, y; // координаты по X и Y
        public static Vector3 Zero => new Vector3(0, 0); // нулевая точка

        public Vector3(float x, float y)
        {
            this.x = x; // сохраняем X
            this.y = y; // сохраняем Y
        }
    }
}