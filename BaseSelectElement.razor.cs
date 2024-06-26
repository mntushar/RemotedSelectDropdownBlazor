using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace DNE.CS.Inventory.Client.Pages.Shared.Component;

public delegate ValueTask<ItemsProviderResult<SelectElement>> BaseSelecterItemsProvider(
    BaseSelectElementRequest request);

public partial class BaseSelectElement
{
    private bool _isSelectItemBoxActive = false;
    private bool _itemClicked = false;
    private string? _searchKey;
    private SelectElement _context = new SelectElement();
    private ElementReference SelectBoxRef;
    private Virtualize<SelectElement>? VirtualRef;
    string? NameSearch
    {
        get => _searchKey;
        set
        {
            if (string.IsNullOrEmpty(value))
                _searchKey = null;
            else
                _searchKey = value;

            if (VirtualRef != null)
                _ = VirtualRef.RefreshDataAsync();
        }
    }
    [Parameter]
    public bool IsMultiple { get; set; } = false;
    [Parameter]
    public string? SelectId { get; set; }
    [Parameter]
    public string? Placeholder { get; set; }
    [Parameter]
    public string? SelectedItem { get; set; }
    [Parameter]
    public Delegate? SelectItemProvider { get; set; }
    [Parameter]
    public EventCallback<SelectElement> SelectedOutput { get; set; }


    private async Task SelectInputFocused()
    {
        _isSelectItemBoxActive = true;
        await SelectBoxRef.FocusAsync();
    }

    private void SearchInputBlurred()
    {
        if (_searchKey == null)
        {
            _isSelectItemBoxActive = false;
            _searchKey = null;
        }
    }

    private void SelectInputBlurred()
    {
        if (_itemClicked)
        {
            _itemClicked = false;
        }
        else
        {
            _isSelectItemBoxActive = false;
            _searchKey = null;
        }
    }

    private void PreventSelectBoxBlur(MouseEventArgs e)
    {
        if (e != null)
            _itemClicked = true;
    }

    private async Task SelectData(SelectElement element)
    {
        _isSelectItemBoxActive = false;
        SelectedItem = element.Name;
        _searchKey = null;
        await SelectedOutput.InvokeAsync(element);
    }

    private async Task CleanData()
    {
        SelectedItem = null;
        _searchKey = null;
        await SelectedOutput.InvokeAsync(new SelectElement());
    }

    public async Task Refresh()
    {
        if (VirtualRef != null)
        {
            await VirtualRef.RefreshDataAsync();
            StateHasChanged();
        }
    }

    private async ValueTask<ItemsProviderResult<SelectElement>> ProvideVirtualizedItemsAync(
        ItemsProviderRequest request)
    {
        if (request.CancellationToken.IsCancellationRequested) return default;

        if (SelectItemProvider != null)
        {
            BaseSelectElementRequest baseSelectElementRequest = new BaseSelectElementRequest()
            {
                StartIndex = request.StartIndex,
                Count = request.Count,
                CancellationToken = request.CancellationToken,
                SearchKey = _searchKey
            };
            object?[]? objects = { baseSelectElementRequest };
            object? result = SelectItemProvider.DynamicInvoke(objects);

            if (result is ValueTask<ItemsProviderResult<SelectElement>> valueTaskResult)
            {
                return await valueTaskResult;
            }
            else
            {
                throw new Exception("SelectItemProvider is not a SelectElement delegate type");
            }
        }

        return new ItemsProviderResult<SelectElement>(new List<SelectElement>(), 0);
    }
}

public class SelectElement
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class BaseSelectElementRequest
{
    public int StartIndex { get; init; }
    public int Count { get; init; }
    public CancellationToken CancellationToken { get; init; }
    public string? SearchKey { get; init; }
}
