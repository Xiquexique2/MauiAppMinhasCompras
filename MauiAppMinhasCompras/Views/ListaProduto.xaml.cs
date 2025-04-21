using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    ObservableCollection<Produto> lista = new ObservableCollection<Produto>();
    List<Produto> todosProdutos = new List<Produto>(); 

    public ListaProduto()
    {
        InitializeComponent();
        lst_produtos.ItemsSource = lista;
    }

    protected async override void OnAppearing()
    {
        try
        {
            lista.Clear();
            todosProdutos.Clear();

            List<Produto> tmp = await App.Db.GetAll();
            todosProdutos.AddRange(tmp); 

            tmp.ForEach(i => lista.Add(i));

            
            var categorias = todosProdutos
                .Select(p => p.Categoria)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToList();

            categorias.Insert(0, "Todas");
            categoriaPicker.ItemsSource = categorias;
            categoriaPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            Navigation.PushAsync(new Views.NovoProduto());
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void txt_search_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            string q = e.NewTextValue;

            lst_produtos.IsRefreshing = true;

            lista.Clear();

            var filtrados = todosProdutos
                .Where(p => p.Descricao.ToLower().Contains(q.ToLower()))
                .ToList();

            filtrados.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }

    private void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        double soma = lista.Sum(i => i.Total);

        string msg = $"O total é {soma:C}";

        DisplayAlert("Total dos Produtos", msg, "OK");
    }

    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecinado = sender as MenuItem;

            Produto p = selecinado.BindingContext as Produto;

            bool confirm = await DisplayAlert(
                "Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "Não");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                lista.Remove(p);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;

            Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p,
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        try
        {
            lista.Clear();
            todosProdutos.Clear();

            List<Produto> tmp = await App.Db.GetAll();
            todosProdutos.AddRange(tmp);

            tmp.ForEach(i => lista.Add(i));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
        finally
        {
            lst_produtos.IsRefreshing = false;
        }
    }

    private void categoriaPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        string categoriaSelecionada = categoriaPicker.SelectedItem as string;

        
        var filtrados = todosProdutos
            .Where(p => categoriaSelecionada == "Todas" || p.Categoria == categoriaSelecionada)
            .ToList();

       
        string pesquisa = txt_search.Text?.ToLower() ?? string.Empty;
        filtrados = filtrados
            .Where(p => p.Descricao.ToLower().Contains(pesquisa))
            .ToList();

        lista.Clear();
        filtrados.ForEach(i => lista.Add(i));
    }
    private async void ToolbarItem_Clicked_Relatorio(object sender, EventArgs e)
    {
        try
        {
            if (!todosProdutos.Any())
            {
                await DisplayAlert("Relatório", "Nenhum produto disponível.", "OK");
                return;
            }

            var relatorio = todosProdutos
                .Where(p => !string.IsNullOrEmpty(p.Categoria))
                .GroupBy(p => p.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(p => p.Total)
                })
                .OrderByDescending(r => r.Total)
                .ToList();

            string msg = string.Join(Environment.NewLine,
                relatorio.Select(r => $"{r.Categoria}: {r.Total:C}"));

            await DisplayAlert("Relatório de Gastos por Categoria", msg, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
    }


}
