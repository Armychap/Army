using ArmyBattle.Models;
using ArmyBattle.UI;
using System.Windows;

namespace ArmyBattle.Game.Commands
{
    /// <summary>
    /// Команда для автоматического прохождения битвы до конца с поддержкой отмены
    /// Выполняет ВСЕ ходы как ОДНУ команду, чтобы Undo откатывал всю авто целиком
    /// </summary>
    public class AutoBattleCommand : ICommand
    {
        public string Name => "Автобой до конца";
        public bool CanUndo => true; // Автобой можно отменить полностью
        
        // Движок битвы, над которым выполняется автоматическое сражение
        private readonly BattleEngine _battle;
        // Окно для обновления UI (может быть null, тогда обновляем только по окончании)
        private readonly BattleWindow? _view;
        
        // Флаг выполнения команды, чтобы не запустить автобой повторно
        private bool _executed;
        
        // Состояние битвы до начала автобоя
        private BattleMemento? _beforeState;
        // Состояние битвы после завершения автобоя
        private BattleMemento? _afterState;
        
        public AutoBattleCommand(BattleEngine battle, BattleWindow? view = null)
        {
            _battle = battle;
            _view = view;
        }
        
        
        // Запускает автоматическое выполнение ходов до завершения битвы
        public void Execute()
        {
            // Защита от повторного выполнения в первый раз
            if (!_executed)
            {
                // Сохраняем состояние перед автобоем
                _beforeState = _battle.CreateMemento();
                
                // Выполняем одиночные ходы, пока битва не закончится
                while (_battle.DoSingleMove())
                {
                    // Обновляем UI с небольшой задержкой для визуального восприятия ходов
                    if (_view != null)
                    {
                        _view.Dispatcher.Invoke(() =>
                        {
                            _view.RenderRosters();
                            _view.RenderFormationField();
                        });
                    }
                    System.Threading.Thread.Sleep(200);
                }
                
                // Сохраняем состояние после автобоя
                _afterState = _battle.CreateMemento();
                _executed = true;
            }
            else
            {
                // Повторное выполнение (Redo) — восстанавливаем состояние ПОСЛЕ авто
                if (_afterState != null)
                {
                    _battle.RestoreMemento(_afterState);
                }
                
                // Обновляем UI
                if (_view != null)
                {
                    _view.Dispatcher.Invoke(() =>
                    {
                        _view.RenderRosters();
                        _view.RenderFormationField();
                    });
                }
            }
        }
        
        // Отмена — возвращаемся к состоянию перед автобоем
        public void Undo()
        {
            if (_beforeState != null)
            {
                _battle.RestoreMemento(_beforeState);
            }
            
            // Обновляем UI
            if (_view != null)
            {
                _view.Dispatcher.Invoke(() =>
                {
                    _view.RenderRosters();
                    _view.RenderFormationField();
                });
            }
        }
    }
}