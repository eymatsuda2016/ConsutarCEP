# Consultar CEP

Aplicação Windows Forms para consultar CEPs e endereços usando a API pública do [ViaCEP](https://viacep.com.br/).

## Recursos

- Consulta rápida por CEP.
- Consulta por endereço com UF, cidade e logradouro.
- Seleção de todos os CEPs encontrados para um logradouro.
- Consulta em lote a partir de um arquivo `.txt`.
- Validação do arquivo em lote, com remoção de inválidos e duplicados.
- Resumo com encontrados, não encontrados, erros e tempo total.
- Exportação individual e em lote para `.txt`, `.csv` e `.xlsx`.
- Histórico de consultas entre sessões.
- Cache configurável de consultas recentes.
- Ajuste do tamanho da fonte e modo escuro.
- Cancelamento de consultas em andamento.
- Verificação automática de atualizações.
- Registro técnico de erros e eventos.

## Como usar

1. Execute `bin\Release\ConsultaCEPs.exe` ou use o atalho criado pelo instalador.
2. Para consultar um CEP, informe-o no campo de busca rápida e pressione `Enter`.
3. Para consultar por endereço, informe UF, cidade e logradouro e clique em **Consultar CEP**.
4. Quando houver mais de um resultado, selecione o CEP desejado no campo **CEPs encontrados**.
5. Use **Copiar CEP** ou **Exportar** para reutilizar o resultado.

Atalhos disponíveis:

- `Enter`: executar a consulta.
- `Esc`: limpar o formulário.

## Consulta em lote

Crie um arquivo `.txt` com um CEP por linha. O sistema aceita CEPs com ou sem hífen, remove duplicidades e ignora entradas inválidas. O limite atual é de 100 CEPs por arquivo.

Ao terminar, escolha entre CSV e Excel (`.xlsx`). O resumo informa a quantidade de endereços encontrados, CEPs não encontrados, erros e o tempo da operação. O botão **Consulta em lote** muda para **Cancelar lote** durante o processamento.

## Configurações

O botão **Configurar** permite alterar:

- tamanho da fonte entre 85% e 125%;
- validade do cache entre 1 e 60 minutos;
- modo escuro.

As preferências ficam salvas em `%LOCALAPPDATA%\ConsultarCEP`.

## Logs e diagnóstico

Os logs técnicos ficam em:

`%LOCALAPPDATA%\ConsultarCEP\Logs\consultar-cep.log`

Em caso de falha, verifique a conexão com a internet, confirme se o CEP ou endereço está correto e consulte esse arquivo para obter detalhes técnicos.

## Compilação e testes

O projeto principal utiliza .NET Framework 4.5 e pode ser compilado no Windows com:

```powershell
& 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe' .\ConsultaCEPs.csproj /t:Build /p:Configuration=Release /v:minimal
```

Os testes automatizados ficam em `Tests` e podem ser executados com:

```powershell
dotnet test .\Tests\ConsultaCEPs.Tests.csproj --verbosity minimal
```

## Criar o instalador

Depois de gerar o Release, execute:

```powershell
.\Installer\Criar-Instalador.ps1
```

O instalador será criado em `bin\Release\ConsultarCEP-Setup.exe`. Ele instala por usuário, cria atalhos na Área de Trabalho e no menu Iniciar e pode ser removido pelas configurações do Windows.

Mais detalhes estão em [Installer/README.md](Installer/README.md).

## Atualizações e distribuição

O projeto possui pipeline de build e testes em `.github\workflows\ci.yml`. Os artefatos Release podem ser publicados em uma GitHub Release. A assinatura digital é opcional e está documentada em [Installer/README.md](Installer/README.md).

## Repositório

<https://github.com/eymatsuda2016/ConsutarCEP>
