using System.Collections.Generic;
using System.Linq;

namespace ShoppingCartWithUndo
{
    // Паттерн Command: Интерфейс команды, инкапсулирующий запрос как объект.
    // Позволяет параметризовать клиентов с различными запросами, ставить запросы в очередь,
    // и поддерживать отмену операций (Undo).
    public interface ICommand
    {
        void Execute(); // Выполнить команду
        void Undo();    // Отменить команду
        string GetDescription(); // Описание команды для логирования
    }

    // Конкретная команда: Добавление товара в корзину
    public class AddToCartCommand : ICommand
    {
        private readonly ShoppingCart _cart;
        private readonly Product _product;
        private readonly int _quantity;

        public AddToCartCommand(ShoppingCart cart, Product product, int quantity)
        {
            _cart = cart;
            _product = product;
            _quantity = quantity;
        }

        public void Execute()
        {
            _cart.AddItem(_product, _quantity);
        }

        public void Undo()
        {
            _cart.RemoveItem(_product.Id);
        }

        public string GetDescription() => $"Добавлен {_product.Name} x{_quantity}";
    }

    // Конкретная команда: Удаление товара из корзины
    public class RemoveFromCartCommand : ICommand
    {
        private readonly ShoppingCart _cart;
        private readonly string _productId;
        private CartItem _removedItem;

        public RemoveFromCartCommand(ShoppingCart cart, string productId)
        {
            _cart = cart;
            _productId = productId;
        }

        public void Execute()
        {
            _removedItem = _cart.Items.FirstOrDefault(i => i.Product.Id == _productId);
            if (_removedItem != null)
            {
                _cart.RemoveItem(_productId);
            }
        }

        public void Undo()
        {
            if (_removedItem != null)
            {
                _cart.AddItem(_removedItem.Product, _removedItem.Quantity);
                _cart.ApplyDiscount(_productId, _removedItem.Discount);
            }
        }

        public string GetDescription() => $"Удален товар {_productId}";
    }

    // Конкретная команда: Изменение количества товара
    public class ChangeQuantityCommand : ICommand
    {
        private readonly ShoppingCart _cart;
        private readonly string _productId;
        private readonly int _newQuantity;
        private int _oldQuantity;

        public ChangeQuantityCommand(ShoppingCart cart, string productId, int newQuantity)
        {
            _cart = cart;
            _productId = productId;
            _newQuantity = newQuantity;
        }

        public void Execute()
        {
            var item = _cart.Items.FirstOrDefault(i => i.Product.Id == _productId);
            if (item != null)
            {
                _oldQuantity = item.Quantity;
                _cart.ChangeQuantity(_productId, _newQuantity);
            }
        }

        public void Undo()
        {
            _cart.ChangeQuantity(_productId, _oldQuantity);
        }

        public string GetDescription() => $"Изменено количество товара {_productId}: {_oldQuantity} -> {_newQuantity}";
    }

    // Конкретная команда: Применение скидки к товару
    public class ApplyDiscountCommand : ICommand
    {
        private readonly ShoppingCart _cart;
        private readonly string _productId;
        private readonly decimal _newDiscount;
        private decimal _oldDiscount;

        public ApplyDiscountCommand(ShoppingCart cart, string productId, decimal discount)
        {
            _cart = cart;
            _productId = productId;
            _newDiscount = discount;
        }

        public void Execute()
        {
            var item = _cart.Items.FirstOrDefault(i => i.Product.Id == _productId);
            if (item != null)
            {
                _oldDiscount = item.Discount;
                _cart.ApplyDiscount(_productId, _newDiscount);
            }
        }

        public void Undo()
        {
            _cart.ApplyDiscount(_productId, _oldDiscount);
        }

        public string GetDescription() => $"Применена скидка ${_newDiscount} к {_productId} (было ${_oldDiscount})";
    }

    // Макрокоманда: Композитная команда, состоящая из нескольких команд.
    // Позволяет выполнять группу команд как одну, с поддержкой Undo для всей группы.
    public class ApplyPromoCodesCommand : ICommand
    {
        private readonly List<ICommand> _discountCommands = new List<ICommand>();
        private readonly ILogger _logger;

        public ApplyPromoCodesCommand(ILogger logger)
        {
            _logger = logger;
        }

        public void AddDiscountCommand(ICommand command)
        {
            _discountCommands.Add(command);
        }

        public void Execute()
        {
            foreach (var cmd in _discountCommands)
            {
                cmd.Execute();
                _logger.Log($"  Выполнено: {cmd.GetDescription()}");
            }
        }

        public void Undo()
        {
            for (int i = _discountCommands.Count - 1; i >= 0; i--)
            {
                _discountCommands[i].Undo();
                _logger.Log($"  Отменено: {_discountCommands[i].GetDescription()}");
            }
        }

        public string GetDescription()
        {
            return $"Применение промокодов ({_discountCommands.Count} скидок)";
        }
    }
}