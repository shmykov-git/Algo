﻿using System;

namespace ViewMotion.Commands;

class Command : UICommand<bool>
{
    public Command(Action execute, Func<bool>? canExecute = null, Action<Action>? buttonRefresher = null) :
        base(_ => execute(), _ => canExecute?.Invoke() ?? true, buttonRefresher)
    {
    }
}