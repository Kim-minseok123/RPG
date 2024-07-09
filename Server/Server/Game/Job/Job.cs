using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public abstract class IJob
	{
		public abstract void Execute();
		public bool Cancel { get; set; } = false;
	}

	public class Job : IJob
	{
		Action _action;

		public Job(Action action)
		{
			_action = action;
		}

		public override void Execute()
		{
			if (Cancel == false)
				_action.Invoke();
		}
	}

	public class Job<T1> : IJob
	{
		Action<T1> _action;
		T1 _t1;

		public Job(Action<T1> action, T1 t1)
		{
			_action = action;
			_t1 = t1;
		}

		public override void Execute()
		{
			if (Cancel == false)
				_action.Invoke(_t1);
		}
	}

	public class Job<T1, T2> : IJob
	{
		Action<T1, T2> _action;
		T1 _t1;
		T2 _t2;

		public Job(Action<T1, T2> action, T1 t1, T2 t2)
		{
			_action = action;
			_t1 = t1;
			_t2 = t2;
		}

		public override void Execute()
		{
			if (Cancel == false)
				_action.Invoke(_t1, _t2);
		}
	}

	public class Job<T1, T2, T3> : IJob
	{
		Action<T1, T2, T3> _action;
		T1 _t1;
		T2 _t2;
		T3 _t3;

		public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
		{
			_action = action;
			_t1 = t1;
			_t2 = t2;
			_t3 = t3;
		}

		public override void Execute()
		{
			if (Cancel == false)
				_action.Invoke(_t1, _t2, _t3);
		}
	}
    public class Job<T1, T2, T3, T4, T5, T6, T7> : IJob
    {
        Action<T1, T2, T3, T4, T5, T6, T7> _action;
        T1 _t1;
        T2 _t2;
        T3 _t3;
        T4 _t4;
        T5 _t5;
        T6 _t6;
        T7 _t7;

        public Job(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
			_t4 = t4;
			_t5 = t5;
			_t6 = t6;
			_t7 = t7;
        }

        public override void Execute()
        {
            if (Cancel == false)
                _action.Invoke(_t1, _t2, _t3, _t4, _t5, _t6, _t7);
        }
    }
}
