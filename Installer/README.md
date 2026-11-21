# Instalador do Consultar CEP

Para criar o instalador gráfico, execute `Criar-Instalador.ps1` após compilar a configuração `Release`. O resultado será `bin\Release\ConsultarCEP-Setup.exe`.

1. Compile a configuração `Release` do projeto.
2. Execute `Instalar-ConsultarCEP.cmd` com duplo clique.
3. O instalador cria atalhos na Área de Trabalho e no menu Iniciar.

A instalação é feita por usuário em `%LOCALAPPDATA%\Programs\ConsultarCEP`, sem exigir privilégios de administrador. Executar o instalador novamente atualiza os arquivos existentes.

A desinstalação pode ser feita em **Configurações > Aplicativos** ou executando `Desinstalar-ConsultarCEP.ps1` dentro da pasta instalada.

## Assinatura digital

Para assinar o executável e o instalador, forneça um certificado de código `.pfx` confiável e execute:

```powershell
.\Assinar-Release.ps1 -CertificatePath .\meu-certificado.pfx -CertificatePassword 'SENHA'
```

O script procura o `signtool.exe` do Windows SDK, usa SHA-256, aplica carimbo de tempo e valida os dois artefatos. A senha não deve ser gravada no repositório nem em scripts versionados.
