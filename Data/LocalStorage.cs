using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.JSInterop;

namespace BoneBaker.Data;

public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;
    private const string CharactersKey = "Characters";

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("StorageHelper.setItem", key, value);
    }

    public async Task<string?> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string?>("StorageHelper.getItem", key);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeVoidAsync("StorageHelper.removeItem", key);
    }

    public async Task ClearAsync()
    {
        await _jsRuntime.InvokeVoidAsync("StorageHelper.clear");
    }

    public async Task<Character[]> GetCharactersAsync()
    {
        var serializedCharacters = await GetItemAsync(CharactersKey);
        if (string.IsNullOrEmpty(serializedCharacters))
        {
            return [];
        }

        return JsonSerializer.Deserialize<Character[]>(serializedCharacters)
            ?? throw new InvalidDataException("Characters appears to be serialized incorrectly");
    }

    public Task SetCharactersAsync(Character[] characters)
    {
        var data = JsonSerializer.Serialize(characters);
        return SetItemAsync(CharactersKey, data);
    }

    public async Task<Character?> TryGetCharacterAsync(Guid id)
    {
        var characters = await GetCharactersAsync();
        return characters.FirstOrDefault(c => c.Identifier == id);
    }
}