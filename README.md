# Lista de Avisos Diversos — Protótipo (WPF / .NET)

Protótipo funcional de um app desktop que substitui a planilha Excel:
busca as respostas do Google Forms (publicadas como CSV) e gera um PDF
pronto para impressão, com quebra de página automática.

## Como abrir

1. Instale o **Visual Studio 2022** (Community é gratuito) com a carga de
   trabalho **".NET Desktop Development"**.
2. Abra a pasta `ListaAvisosApp` no Visual Studio (`Arquivo > Abrir > Pasta`)
   ou crie um novo projeto WPF e copie estes arquivos para dentro.
3. Restaure os pacotes NuGet (acontece automaticamente ao abrir, ou
   clique com botão direito na solução > "Restaurar Pacotes NuGet").
4. Pressione F5 para rodar.

## Passo obrigatório antes de usar: publicar a planilha como CSV

1. Abra a planilha Google Sheets que recebe as respostas do seu Forms.
2. `Arquivo > Compartilhar > Publicar na Web`.
3. Escolha a aba correta e o formato **CSV**.
4. Copie a URL gerada (algo como
   `https://docs.google.com/spreadsheets/d/e/2PACX-.../pub?output=csv`)
   e cole no campo `PlanilhaCsvUrl` do arquivo `appsettings.json`.
5. Ajuste também `NomeIgreja` para o nome que deve aparecer no cabeçalho
   do relatório.

## Sobre o mapeamento de colunas

O Google Forms nomeia as colunas da planilha com o **texto da própria
pergunta** (ex.: "Qual a data do evento?"). Por isso, o
`EventoService.cs` procura a coluna que **contém** uma palavra-chave
(`data`, `hor`, `tipo`, `descri`, `respons`, `local`) em vez de exigir um
nome exato.

Se o seu formulário usa perguntas bem diferentes dessas palavras-chave,
ajuste as chamadas de `ColunaQueContem(...)` no início do método
`BuscarEventosAsync` em `Services/EventoService.cs`.

## Estrutura do projeto

```
ListaAvisosApp/
  Models/Evento.cs          -> modelo de dados de um evento
  Services/EventoService.cs -> busca e converte o CSV em lista de eventos
  Reports/ListaAvisosReport.cs -> layout do PDF (QuestPDF)
  MainWindow.xaml(.cs)      -> tela única: Atualizar + Gerar/Imprimir
  App.xaml(.cs)             -> inicialização, licença do QuestPDF
  appsettings.json          -> URL da planilha e nome da igreja
```

## Personalizando o relatório

O layout está todo em `Reports/ListaAvisosReport.cs`. Para mudar fontes,
cores, colunas da tabela ou agrupamento, edite esse arquivo — é código
declarativo (fluent API), fácil de ler e ajustar.

## Gerando o instalador (para a máquina do secretário)

1. Publique o app como um único executável, sem depender do .NET
   instalado na máquina de destino:

   ```
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```

   O executável sai em
   `bin/Release/net8.0-windows/win-x64/publish/ListaAvisosApp.exe`.

2. (Opcional, recomendado) Use o **Inno Setup** (gratuito,
   jrsoftware.org/isinfo.php) para empacotar esse `.exe` + o
   `appsettings.json` num instalador `.exe` único, com atalho na área de
   trabalho — assim o secretário só clica duas vezes para instalar.

## Próximos passos sugeridos

- Adicionar uma tela de configuração simples (em vez de editar o JSON
  na mão) para trocar a URL da planilha e o nome da igreja pela própria
  interface.
- Adicionar filtro por período (ex.: "próximos 30 dias") na tela
  principal.
- Se quiser esconder a URL da planilha do público, trocar o CSV
  publicado pela Google Sheets API com uma credencial de serviço.
