using System;
using System.Diagnostics;

using NCrawler.Extensions;

namespace NCrawler.Utils
{
	public class AspectF
	{
		#region Fields

		/// <summary>
		/// Chain of aspects to invoke
		/// </summary>
		internal Action<Action> m_Chain;

		/// <summary>
		/// The acrual work delegate that is finally called
		/// </summary>
		internal Delegate m_WorkDelegate;

		#endregion

		#region Instance Methods

		/// <summary>
		/// Create a composition of function e.g. f(g(x))
		/// </summary>
		/// <param name="newAspectDelegate">A delegate that offers an aspect's behavior. 
		/// It's added into the aspect chain</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public AspectF Combine(Action<Action> newAspectDelegate)
		{
			if (this.m_Chain.IsNull())
			{
                this.m_Chain = newAspectDelegate;
			}
			else
			{
				Action<Action> existingChain = this.m_Chain;
				Action<Action> callAnother = work => existingChain(() => newAspectDelegate(work));
                this.m_Chain = callAnother;
			}

			return this;
		}

		/// <summary>
		/// Execute your real code applying the aspects over it
		/// </summary>
		/// <param name="work">The actual code that needs to be run</param>
		[DebuggerStepThrough]
		public void Do(Action work)
		{
			if (this.m_Chain.IsNull())
			{
				work();
			}
			else
			{
                this.m_Chain(work);
			}
		}

		/// <summary>
		/// Execute your real code applying the aspects over it
		/// </summary>
		/// <param name="work">The actual code that needs to be run</param>
		[DebuggerStepThrough]
		public void Do<TParam1>(Action<TParam1> work) where TParam1 : IDisposable, new()
		{
			using (var p = new TParam1())
			{
                this.Do(p, work);
			}
		}

		/// <summary>
		/// Execute your real code applying the aspects over it
		/// </summary>
		/// <param name="p"></param>
		/// <param name="work">
		/// 	The actual code that needs to be run
		/// </param>
		[DebuggerStepThrough]
		public void Do<TParam1>(TParam1 p, Action<TParam1> work)
		{
			if (this.m_Chain.IsNull())
			{
				work(p);
			}
			else
			{
                this.m_Chain(() => work(p));
			}
		}

		/// <summary>
		/// Execute your real code applying aspects over it.
		/// </summary>
		/// <typeparam name="TReturnType"></typeparam>
		/// <param name="work">The actual code that needs to be run</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public TReturnType Return<TReturnType>(Func<TReturnType> work)
		{
            this.m_WorkDelegate = work;

			if (this.m_Chain.IsNull())
			{
				return work();
			}

			var returnValue = default(TReturnType);
            this.m_Chain(() =>
			{
				Func<TReturnType> workDelegate = (Func<TReturnType>)this.m_WorkDelegate;
				returnValue = workDelegate();
			});
			return returnValue;
		}

		/// <summary>
		/// Execute your real code applying aspects over it.
		/// </summary>
		/// <typeparam name="TReturnType"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <param name="work">The actual code that needs to be run</param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public TReturnType Return<TReturnType, TParam1>(Func<TParam1, TReturnType> work) where TParam1 : IDisposable, new()
		{
			using (var p = new TParam1())
			{
				return this.Return(p, work);
			}
		}

		/// <summary>
		/// Execute your real code applying aspects over it.
		/// </summary>
		/// <typeparam name="TReturnType"></typeparam>
		/// <typeparam name="TParam1"></typeparam>
		/// <param name="p"></param>
		/// <param name="work">
		/// 	The actual code that needs to be run
		/// </param>
		/// <returns></returns>
		[DebuggerStepThrough]
		public TReturnType Return<TReturnType, TParam1>(TParam1 p, Func<TParam1, TReturnType> work)
		{
            this.m_WorkDelegate = work;

			if (this.m_Chain.IsNull())
			{
				return work(p);
			}

			var returnValue = default(TReturnType);
            this.m_Chain(() =>
			{
				Func<TParam1, TReturnType> workDelegate = (Func<TParam1, TReturnType>)this.m_WorkDelegate;
				returnValue = workDelegate(p);
			});

			return returnValue;
		}

		#endregion

		#region Class Properties

		/// <summary>
		/// Handy property to start writing aspects using fluent style
		/// </summary>
		public static AspectF Define
		{
			[DebuggerStepThrough]
			get { return new AspectF(); }
		}

		#endregion
	}
}