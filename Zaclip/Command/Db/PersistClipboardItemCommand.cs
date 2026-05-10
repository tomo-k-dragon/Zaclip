using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Zaclip.Db;
using Zaclip.Models;

namespace Zaclip.Command.Db
{
    internal class PersistClipboardItemCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;
        public bool CanExecute(object? parameter)
        {
            if (parameter is not ClipboardItem item)
            {
                return false;
            }

            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is not ClipboardItem item)
            {
                return;
            }
            using (var db = new AppDbContext())
            {
                var target = db.ClipItems.First(x => x.Id == item.Id);
                var newPersisted = !target.Persisted;
                target.Persisted = newPersisted;
                db.SaveChanges();
                item.Persisted = newPersisted;
            }
        }
    }
}
