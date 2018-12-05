// Developed by Softeq Development Corporation
// http://www.softeq.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Softeq.XToolkit.Common.Collections;
using Softeq.XToolkit.Common.Models;
using Softeq.XToolkit.WhiteLabel.Interfaces;
using Softeq.XToolkit.WhiteLabel.Mvvm;

namespace Softeq.XToolkit.Chat.ViewModels
{
    public class PaginationViewModel<TViewModel, TModel>
        where TViewModel : ObservableObject, IViewModelParameter<TModel>
    {
        private readonly IViewModelFactoryService _viewModelFactoryService;
        private readonly Func<int, int, Task<PagingModel<TModel>>> _loaderAction;
        private readonly Action<IReadOnlyList<TViewModel>> _filterAction;
        private readonly int _pageSize;

        private int _pageNumber = 1;

        public PaginationViewModel(
            IViewModelFactoryService viewModelFactory,
            Func<int, int, Task<PagingModel<TModel>>> loaderAction,
            Action<IReadOnlyList<TViewModel>> filterAction,
            int pageSize)
        {
            _viewModelFactoryService = viewModelFactory ?? throw new ArgumentNullException(nameof(viewModelFactory));
            _loaderAction = loaderAction ?? throw new ArgumentNullException(nameof(loaderAction));
            _filterAction = filterAction;
            _pageSize = pageSize;
        }
        
        public ObservableRangeCollection<TViewModel> Items { get; } = new ObservableRangeCollection<TViewModel>();

        public async Task LoadFirstPageAsync()
        {
            _pageNumber = 1;

            var viewModels = await LoadPageAsync(_pageNumber).ConfigureAwait(false);
            
            if (viewModels.Count > 0)
            {
                Items.ReplaceRange(viewModels);
            }
        }
        
        public async Task LoadNextPageAsync()
        {
            Interlocked.Increment(ref _pageNumber);
            
            var viewModels = await LoadPageAsync(_pageNumber).ConfigureAwait(false);

            if (viewModels.Count > 0)
            {
                Items.AddRange(viewModels);
            }
            else
            {
                Interlocked.Decrement(ref _pageNumber);
            }
        }

        private async Task<IReadOnlyList<TViewModel>> LoadPageAsync(int pageNumber)
        {
            var pagingModel = await _loaderAction(pageNumber, _pageSize).ConfigureAwait(false);
            if (pagingModel == null)
            {
                return new List<TViewModel>();
            }

            if (pagingModel.Page == pagingModel.TotalNumberOfPages &&
                pagingModel.Data.Count == 0)
            {
                return new List<TViewModel>();
            }
            
            var viewModels = pagingModel.Data
                .Select(_viewModelFactoryService.ResolveViewModel<TViewModel, TModel>)
                .ToList();

            _filterAction?.Invoke(viewModels);

            return viewModels;
        }
    }
}