using MySql.Data.MySqlClient;
using Pagamento.Models;
using System;
using System.Collections.Generic;

namespace Pagamento.DAO
{
    public class CompraDAO
    {
        private readonly string connectionString = "server=localhost;database=pagamento;user=User;password=Na@der!1234";

        // --- INÍCIO DA MODIFICAÇÃO (Adicionar DAOs) ---
        // DAOs necessários para gerar as parcelas
        private readonly ParcelaCondicaoPagamentoDAO _parcelaCondPgtoDAO = new ParcelaCondicaoPagamentoDAO();
        private readonly ContaAPagarDAO _contaAPagarDAO = new ContaAPagarDAO();
        private readonly CondicaoPagamentoDAO _condicaoPagamentoDAO = new CondicaoPagamentoDAO();
        // --- FIM DA MODIFICAÇÃO ---

        // --- 1. ADICIONE ESTES DAOs ---
        private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
        private readonly ProdutoFornecedorDAO _produtoFornecedorDAO = new ProdutoFornecedorDAO();
        // --- FIM DA MODIFICAÇÃO ---
        // Dentro da sua classe Pagamento.DAO.CompraDAO

        /// <summary>
        /// Lista todas as compras cadastradas, trazendo os dados do fornecedor e os itens de cada compra.
        /// </summary>
        public List<Compra> Listar()
        {
            var listaDeCompras = new List<Compra>();
            using (var conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();

                    // 1. Consulta principal: Busca todas as compras e junta com Fornecedor para pegar o nome.
                    string sqlPrincipal = @"SELECT 
                                        c.*, 
                                        f.Nome_RazaoSocial AS NomeFornecedor 
                                    FROM Compra c
                                    JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor
                                    ORDER BY c.DataEmissao DESC";

                    var cmdPrincipal = new MySqlCommand(sqlPrincipal, conexao);
                    var readerPrincipal = cmdPrincipal.ExecuteReader();

                    while (readerPrincipal.Read())
                    {
                        var compra = new Compra
                        {
                            Modelo = readerPrincipal["Modelo"].ToString(),
                            Serie = readerPrincipal["Serie"].ToString(),
                            NumeroNota = Convert.ToInt32(readerPrincipal["NumeroNota"]),
                            FornecedorId = Convert.ToInt32(readerPrincipal["FornecedorId"]),
                            DataEmissao = Convert.ToDateTime(readerPrincipal["DataEmissao"]),
                            DataChegada = Convert.ToDateTime(readerPrincipal["DataChegada"]),
                            CondicaoPagamentoId = Convert.ToInt32(readerPrincipal["CondicaoPagamentoId"]),
                            Frete = Convert.ToDecimal(readerPrincipal["Frete"]),
                            Seguro = Convert.ToDecimal(readerPrincipal["Seguro"]),
                            Despesas = Convert.ToDecimal(readerPrincipal["Despesas"]),
                            Status = Convert.ToBoolean(readerPrincipal["Status"]),
                            NomeFornecedor = readerPrincipal["NomeFornecedor"].ToString()
                        };
                        listaDeCompras.Add(compra);
                    }
                    readerPrincipal.Close(); // Fecha o primeiro reader antes de abrir outros.

                    // 2. Para cada compra na lista, busca seus itens.
                    foreach (var compra in listaDeCompras)
                    {
                        string sqlItens = @"SELECT 
                                        ic.*, 
                                        p.Descricao AS NomeProduto, 
                                        um.Descricao AS NomeUnidade,
                                        ic.CustoUnitarioReal
                                    FROM ItemCompra ic
                                    JOIN Produto p ON ic.ProdutoId = p.IdProduto
                                    JOIN UnidadeMedida um ON p.UnidadeMedidaId = um.IdUnidadeMedida
                                    WHERE 
                                        ic.CompraModelo = @Modelo AND
                                        ic.CompraSerie = @Serie AND
                                        ic.CompraNumeroNota = @NumeroNota AND
                                        ic.CompraFornecedorId = @FornecedorId";

                        var cmdItens = new MySqlCommand(sqlItens, conexao);
                        cmdItens.Parameters.AddWithValue("@Modelo", compra.Modelo);
                        cmdItens.Parameters.AddWithValue("@Serie", compra.Serie);
                        cmdItens.Parameters.AddWithValue("@NumeroNota", compra.NumeroNota);
                        cmdItens.Parameters.AddWithValue("@FornecedorId", compra.FornecedorId);

                        var readerItens = cmdItens.ExecuteReader();
                        while (readerItens.Read())
                        {
                            var item = new ItemCompra
                            {
                                ProdutoId = Convert.ToInt32(readerItens["ProdutoId"]),
                                Quantidade = Convert.ToInt32(readerItens["Quantidade"]),
                                ValorUnitario = Convert.ToDecimal(readerItens["ValorUnitario"]),
                                NomeProduto = readerItens["NomeProduto"].ToString(),
                                NomeUnidade = readerItens["NomeUnidade"].ToString(),
                                // 2. MAPEIE A PROPRIEDADE
                                CustoUnitarioReal = Convert.ToDecimal(readerItens["CustoUnitarioReal"])
                                // --- FIM DA MODIFICAÇÃO DO MAPEAMENTO ---
                            };
                            compra.Itens.Add(item);
                        }
                        readerItens.Close(); // Fecha o reader de itens.
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao listar as compras. Detalhes: {ex.Message}");
                }
            }
            return listaDeCompras;
        }

        public void Inserir(Compra compra)
        {
            // O 'using' garante que a conexão será fechada ao final do bloco.
            using (MySqlConnection conexao = new MySqlConnection(connectionString))
            {
                try
                {
                    conexao.Open();

                    var custosRateados = RatearCustosAdicionais(compra);

                    // --- PASSO 1: Inserir a Compra ---
                    string sqlCompra = @"INSERT INTO Compra 
                                         (Modelo, Serie, NumeroNota, FornecedorId, DataEmissao, DataChegada, CondicaoPagamentoId, Frete, Seguro, Despesas, Status,DataCriacao) 
                                         VALUES 
                                         (@Modelo, @Serie, @NumeroNota, @FornecedorId, @DataEmissao, @DataChegada, @CondicaoPagamentoId, @Frete, @Seguro, @Despesas, @Status,@DataCriacao)";

                    // Note que o parâmetro 'transaction' foi removido do construtor do MySqlCommand
                    MySqlCommand cmdCompra = new MySqlCommand(sqlCompra, conexao);
                    cmdCompra.Parameters.AddWithValue("@Modelo", compra.Modelo);
                    cmdCompra.Parameters.AddWithValue("@Serie", compra.Serie);
                    cmdCompra.Parameters.AddWithValue("@NumeroNota", compra.NumeroNota);
                    cmdCompra.Parameters.AddWithValue("@FornecedorId", compra.FornecedorId);
                    cmdCompra.Parameters.AddWithValue("@DataEmissao", compra.DataEmissao);
                    cmdCompra.Parameters.AddWithValue("@DataChegada", compra.DataChegada);
                    cmdCompra.Parameters.AddWithValue("@CondicaoPagamentoId", compra.CondicaoPagamentoId);
                    cmdCompra.Parameters.AddWithValue("@Frete", compra.Frete);
                    cmdCompra.Parameters.AddWithValue("@Seguro", compra.Seguro);
                    cmdCompra.Parameters.AddWithValue("@Despesas", compra.Despesas);
                    cmdCompra.Parameters.AddWithValue("@Status", compra.Status);
                    cmdCompra.Parameters.AddWithValue("@DataCriacao", DateTime.Now);

                    cmdCompra.ExecuteNonQuery();

                    // --- PASSO 2: Inserir cada Item da Compra ---
                    foreach (var item in compra.Itens)
                    {

                        decimal custoRealRateado = custosRateados.TryGetValue(item.ProdutoId, out var v)
                                                                                ? v : item.ValorUnitario;

                        decimal custoAdicUnit = Math.Round(
                            custoRealRateado - item.ValorUnitario, 2, MidpointRounding.AwayFromZero);
                        decimal custoRealUnit = Math.Round(
                            custoRealRateado, 2, MidpointRounding.AwayFromZero);

                        string sqlItem = @"INSERT INTO ItemCompra 
                                           (CompraModelo, CompraSerie, CompraNumeroNota, CompraFornecedorId, ProdutoId, Quantidade, ValorUnitario, CustoAdicionalUnitario, CustoUnitarioReal) 
                                           VALUES 
                                           (@CompraModelo, @CompraSerie, @CompraNumeroNota, @CompraFornecedorId, @ProdutoId, @Quantidade, @ValorUnitario, @CustoAdicionalUnitario, @CustoUnitarioReal)";

                        MySqlCommand cmdItem = new MySqlCommand(sqlItem, conexao);
                        cmdItem.Parameters.AddWithValue("@CompraModelo", compra.Modelo);
                        cmdItem.Parameters.AddWithValue("@CompraSerie", compra.Serie);
                        cmdItem.Parameters.AddWithValue("@CompraNumeroNota", compra.NumeroNota);
                        cmdItem.Parameters.AddWithValue("@CompraFornecedorId", compra.FornecedorId);
                        cmdItem.Parameters.AddWithValue("@ProdutoId", item.ProdutoId);
                        cmdItem.Parameters.AddWithValue("@Quantidade", item.Quantidade);
                        cmdItem.Parameters.AddWithValue("@ValorUnitario", item.ValorUnitario);
                        cmdItem.Parameters.AddWithValue("@CustoAdicionalUnitario", Math.Round(custoRealRateado - item.ValorUnitario, 2));
                        cmdItem.Parameters.AddWithValue("@CustoUnitarioReal", Math.Round(custoRealRateado, 2));
                        cmdItem.ExecuteNonQuery(); // Executa e confirma imediatamente
                    }

                    // --- PASSO 3: CALCULAR CUSTOS E ATUALIZAR PRODUTOS E FORNECEDORES ---
                    // Esta lógica também executará comando por comando, sem a proteção da transação.
                    var produtoDAO = new ProdutoDAO(); // Assume que os métodos do ProdutoDAO gerenciam sua própria conexão
                    var produtoFornecedorDAO = new ProdutoFornecedorDAO();

                    foreach (var item in compra.Itens)
                    {
                        if (custosRateados.TryGetValue(item.ProdutoId, out decimal custoRealRateado))
                        {



                            // ======================================================================
                            // ✅ LÓGICA ADICIONADA AQUI
                            // Antes de atualizar, garantimos que a associação existe.
                            // ======================================================================
                            produtoFornecedorDAO.GarantirAssociacao(item.ProdutoId, compra.FornecedorId);


                            // A. Atualiza ProdutoFornecedor
                            produtoFornecedorDAO.AtualizarDadosCompra(item.ProdutoId, compra.FornecedorId, custoRealRateado, compra.DataEmissao);

                            // B. Atualiza Estoque e Custo do Produto
                            Produto produtoAtual = produtoDAO.BuscarPorId(item.ProdutoId);
                            if (produtoAtual != null)
                            {
                                int qtdAntiga = produtoAtual.Quantidade;
                                decimal custoMedioAntigo = produtoAtual.CustoMedio;
                                int qtdNova = item.Quantidade;
                                int novaQtdTotal = qtdAntiga + qtdNova;
                                decimal novoCustoMedio = (qtdAntiga > 0)
                                                        ? ((qtdAntiga * custoMedioAntigo) + (qtdNova * custoRealRateado)) / (qtdAntiga + qtdNova)
                                                        : custoRealRateado;
                                if (novaQtdTotal > 0 && qtdAntiga > 0)
                                {
                                    novoCustoMedio = ((qtdAntiga * custoMedioAntigo) + (qtdNova * custoRealRateado)) / novaQtdTotal;
                                }
                                produtoDAO.AtualizarEstoqueECusto(item.ProdutoId, novaQtdTotal, novoCustoMedio, custoRealRateado);
                            }
                        }
                    }


                    // --- INÍCIO DA MODIFICAÇÃO (PASSO 4: GERAR CONTAS A PAGAR) ---


                    // 1. Buscar a Condição de Pagamento completa (para Juros, Multa, Desconto)
                    // (Usando o DAO que adicionamos no início da classe)
                    var condicao = _condicaoPagamentoDAO.BuscarPorId(compra.CondicaoPagamentoId);
                    if (condicao == null)
                    {
                        // Se a condição não for encontrada, algo está muito errado.
                        // Lançar uma exceção interrompe o processo (o que é bom, pois não há transação para reverter)
                        throw new Exception($"A Condição de Pagamento com ID {compra.CondicaoPagamentoId} não foi encontrada.");
                    }


                    // 1. Buscar as parcelas definidas para a Condição de Pagamento
                    var parcelasDefinidas = _parcelaCondPgtoDAO.ListarPorCondicaoPagamento(compra.CondicaoPagamentoId);

                    // 2. Calcular o valor total da nota (TotalProdutos + Frete + Seguro + Despesas)
                    //    O seu modelo Compra.cs já tem a propriedade 'TotalNota'
                    decimal totalNota = compra.TotalNota;

                    // 3. Iterar sobre as parcelas definidas e criar as Contas a Pagar
                    foreach (var parcelaDef in parcelasDefinidas)
                    {
                        // Calcula o valor desta parcela
                        decimal valorParcela = totalNota * (parcelaDef.ValorPercentual / 100);

                        // Calcula a data de vencimento
                        DateTime dataVencimento = compra.DataEmissao.AddDays(parcelaDef.DiasAposVenda);

                        var contaAPagar = new ContaAPagar
                        {
                            // Chave da Compra
                            Modelo = compra.Modelo,
                            Serie = compra.Serie,
                            NumeroNota = compra.NumeroNota,
                            FornecedorId = compra.FornecedorId,

                            // Dados da Parcela
                            NumeroParcela = parcelaDef.NumeroParcela,
                            ValorParcela = Math.Round(valorParcela, 2), // Arredonda para 2 casas decimais
                            DataVencimento = dataVencimento,
                            DataEmissao = compra.DataEmissao, // Data de emissão da nota original

                            // Status Iniciais
                            Status = true, // Define como 'Pendente' (ou false, dependendo da sua regra)
                            Situacao = "A PAGAR", // Define a situação inicial


                            // --- NOVOS CAMPOS (Termos) ---
                            // Copiando as regras da Condição de Pagamento
                            // (Seu modelo ContaAPagar.cs usa decimais nuláveis (decimal?))
                            Juros = condicao.Juros,
                            Multa = condicao.Multa,
                            Desconto = condicao.Desconto,
                            // Copiando a forma de pagamento da definição da parcela
                            // (Seu modelo ContaAPagar.cs usa int nulável (int?))
                            IdFormaPgto = parcelaDef.IdFormaPgto
                        };

                        // 4. Inserir a parcela no banco de dados (usando o DAO modificado)
                        _contaAPagarDAO.Inserir(contaAPagar);
                    }
                    // --- FIM DA MODIFICAÇÃO ---

                }
                catch (Exception ex)
                {
                    // Sem transação, não há o que reverter (Rollback).
                    // Os dados já salvos permanecerão no banco.
                    throw new Exception("Ocorreu um erro durante a inserção da compra. O processo foi interrompido, mas alguns dados podem ter sido salvos. Detalhes: " + ex.Message);
                }
            }
        }

        // O método RatearCustosAdicionais permanece o mesmo.
        private Dictionary<int, decimal> RatearCustosAdicionais(Compra compra)
        {
            decimal valorTotalItens = compra.Itens.Sum(item => item.ValorUnitario * item.Quantidade);

            if (valorTotalItens == 0) return new Dictionary<int, decimal>();

            decimal totalCustosAdicionais = compra.Frete + compra.Seguro + compra.Despesas;
            var custosReais = new Dictionary<int, decimal>();

            foreach (var item in compra.Itens)
            {
                decimal valorTotalDoItem = item.ValorUnitario * item.Quantidade;
                decimal proporcao = valorTotalDoItem / valorTotalItens;
                decimal custoAdicionalDoItem = totalCustosAdicionais * proporcao;
                decimal custoAdicionalUnitario = (item.Quantidade > 0) ? custoAdicionalDoItem / item.Quantidade : 0;
                decimal custoRealUnitario = item.ValorUnitario + custoAdicionalUnitario;
                custosReais.Add(item.ProdutoId, custoRealUnitario);
            }

            return custosReais;
        }

        //private Dictionary<int, decimal> RatearCustosAdicionais(Compra compra)
        //{
        //    decimal baseTotal = compra.Itens.Sum(i => i.ValorUnitario * i.Quantidade);
        //    if (baseTotal <= 0) return new Dictionary<int, decimal>();

        //    decimal adicionais = compra.Frete + compra.Seguro + compra.Despesas;
        //    var custos = new Dictionary<int, decimal>();

        //    foreach (var item in compra.Itens)
        //    {
        //        decimal baseItem = item.ValorUnitario * item.Quantidade;
        //        decimal proporcao = baseItem / baseTotal;
        //        decimal adicionalDoItem = adicionais * proporcao;

        //        decimal custoAdicionalUnit = (item.Quantidade > 0) ? adicionalDoItem / item.Quantidade : 0m;
        //        decimal custoRealUnit = item.ValorUnitario + custoAdicionalUnit;

        //        // use o ID da linha, não o ProdutoId, para evitar colisões
        //        custos[item.Id] = decimal.Round(custoRealUnit, 6, MidpointRounding.AwayFromZero);
        //    }

        //    return custos;
        //}

        // Dentro de CompraDAO.cs

        // Retorna true se já existir uma compra com a chave informada, false caso contrário.
        public bool ExisteChaveComposta(string modelo, string serie, string numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                // Usamos COUNT(*) que é eficiente para apenas verificar existência
                string sql = @"
                                SELECT COUNT(*) 
                                FROM Compra 
                                WHERE Modelo = @Modelo 
                                  AND Serie = @Serie 
                                  AND NumeroNota = @NumeroNota 
                                  AND FornecedorId = @FornecedorId";

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo ?? (object)DBNull.Value); // Trata null para evitar erros
                cmd.Parameters.AddWithValue("@Serie", serie ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                // ExecuteScalar retorna o valor da primeira coluna da primeira linha (o COUNT)
                var count = Convert.ToInt64(cmd.ExecuteScalar());

                return count > 0; // Retorna true se encontrou 1 ou mais registros
            }
            // Considerar adicionar try-catch para lidar com erros de banco
        }


        // Busca os detalhes de uma Compra (incluindo nome do fornecedor) pela chave composta.
        // Retorna o objeto Compra preenchido ou null se não encontrar.
        public Compra BuscarDetalhesPorChaveComposta(string modelo, string serie, string numeroNota, int fornecedorId)
        {
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();

                // --- SQL CORRIGIDO COM BASE NO SCHEMA ---
                string sql = @"
                            SELECT 
                                c.DataEmissao, c.DataChegada, c.Modelo, c.Serie, c.NumeroNota, 
                                c.FornecedorId, 
                                f.Nome_RazaoSocial AS FornecedorNome, -- Pega o nome da tabela Fornecedor
                                c.Observacao,
                                c.Status
                                -- Adicione outras colunas da tabela Compra se precisar exibi-las
                                -- Ex: c.Status, c.DataCriacao
                            FROM Compra c
                            JOIN Fornecedor f ON c.FornecedorId = f.IdFornecedor -- JOIN CORRETO com Fornecedor
                            WHERE c.Modelo = @Modelo 
                              AND c.Serie = @Serie 
                              AND c.NumeroNota = @NumeroNota 
                              AND c.FornecedorId = @FornecedorId
                            LIMIT 1"; // LIMIT 1 é uma boa prática aqui
                                      // ------------------------------------------

                var cmd = new MySqlCommand(sql, conexao);
                cmd.Parameters.AddWithValue("@Modelo", modelo ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Serie", serie ?? (object)DBNull.Value);

                // -- Tratamento explícito para NumeroNota (INT no banco) --
                if (int.TryParse(numeroNota, out int numeroNotaInt))
                {
                    cmd.Parameters.AddWithValue("@NumeroNota", numeroNotaInt);
                }
                else
                {
                    // Se a string não for um número válido, passe DBNull. 
                    // Uma chave primária INT não deveria ser nula, mas é mais seguro que falhar.
                    cmd.Parameters.AddWithValue("@NumeroNota", DBNull.Value);
                }
                // --------------------------------------------------------

                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                try // Adicionar try-catch para diagnosticar erros de execução SQL
                {
                    var reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Verifique o tipo da propriedade NumeroNota na sua classe Compra.cs
                        int numeroNotaString = reader.GetInt32("NumeroNota");       // Se a propriedade for int

                        // Verifique o nome da propriedade Observacoes/Observacao na classe Compra.cs
                        string observacaoLida = reader.IsDBNull(reader.GetOrdinal("Observacao")) ? null : reader.GetString("Observacao");

                        return new Compra
                        {
                            Modelo = reader.GetString("Modelo"),
                            Serie = reader.GetString("Serie"),
                            NumeroNota = numeroNotaString, // Use a variável correta (string ou int) conforme seu Model
                            FornecedorId = reader.GetInt32("FornecedorId"),
                            DataEmissao = reader.GetDateTime("DataEmissao"),
                            DataChegada = reader.GetDateTime("DataChegada"),
                            NomeFornecedor = reader.GetString("FornecedorNome"), // Mapeia o nome do fornecedor vindo do JOIN
                            Observacoes = observacaoLida, // Mapeia para a propriedade correta (Observacoes ou Observacao)
                            Status = reader.GetBoolean("Status")    // Mapeie outras colunas que você selecionou no SQL (ex: Status)
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Logar o erro detalhado para saber EXATAMENTE o que falhou no SQL ou mapeamento
                    Console.WriteLine($"Erro em BuscarDetalhesPorChaveComposta - DB Query/Read: {ex.ToString()}"); // Log detalhado
                                                                                                                   // Rethrow ou trate o erro conforme necessário, mas LOGUE!
                    throw; // Re-lança a exceção para que o Controller a capture no try-catch dele
                }
            }
            return null; // Retorna null se não encontrou ou se houve erro antes do return
        }

        public void Cancelar(string modelo, string serie, int numeroNota, int fornecedorId, string motivoCancelamento)
        {
            // --- PASSO 1: VALIDAÇÃO FINANCEIRA ---
            // Verifica se alguma parcela desta nota já foi PAGA.
            if (_contaAPagarDAO.VerificarParcelasPagas(modelo, serie, numeroNota, fornecedorId))
            {
                throw new Exception("Não é possível cancelar a compra. Existem parcelas que já foram pagas.");
            }

            // --- PASSO 2: BUSCAR OS DADOS DA COMPRA ---
            // Precisamos dos itens e, principalmente, do 'CustoUnitarioReal' de cada item.
            // Reutilizamos o Listar() pois ele já busca os itens (incluindo o CustoUnitarioReal que adicionamos)
            var compra = this.Listar()
                .FirstOrDefault(c =>
                    c.Modelo == modelo &&
                    c.Serie == serie &&
                    c.NumeroNota == numeroNota &&
                    c.FornecedorId == fornecedorId);

            if (compra == null || compra.Itens == null || !compra.Itens.Any())
            {
                throw new Exception("Compra não encontrada ou sem itens para reverter.");
            }

            // --- PASSO 3: REVERTER ESTOQUE E CUSTO MÉDIO (Para cada item) ---
            foreach (var itemCancelado in compra.Itens)
            {
                Produto produtoAtual = _produtoDAO.BuscarPorId(itemCancelado.ProdutoId);
                if (produtoAtual == null) continue;

                int qtdAtual = produtoAtual.Quantidade;
                decimal custoMedioAtual = produtoAtual.CustoMedio;

                int qtdCancelada = itemCancelado.Quantidade;
                decimal custoRealCancelado = itemCancelado.CustoUnitarioReal; // <-- A CHAVE DA LÓGICA

                // A. Reverte a Quantidade
                int novaQtdTotal = qtdAtual - qtdCancelada;
                if (novaQtdTotal < 0)
                {
                    // Isso significa que o produto foi vendido/usado. 
                    // O sistema deve decidir se bloqueia o cancelamento ou permite estoque negativo.
                    // Por segurança, vamos lançar um erro.
                    throw new Exception($"Não é possível cancelar: O produto '{produtoAtual.Descricao}' ficaria com estoque negativo ({novaQtdTotal}).");
                }

                // B. Reverte o Custo Médio Ponderado
                decimal novoCustoMedio = 0;
                if (novaQtdTotal > 0)
                {
                    decimal valorTotalEstoqueAtual = qtdAtual * custoMedioAtual;
                    decimal valorTotalEstoqueCancelado = qtdCancelada * custoRealCancelado;

                    decimal novoValorTotalEstoque = valorTotalEstoqueAtual - valorTotalEstoqueCancelado;

                    // Evita divisão por zero se o novo valor total for 0
                    novoCustoMedio = (novoValorTotalEstoque > 0) ? (novoValorTotalEstoque / novaQtdTotal) : 0;
                }
                // Se novaQtdTotal == 0, o custo médio é 0.

                // C. Reverter o CustoUltimaCompra
                // Esta é a parte mais complexa. Para fazer 100% correto, precisaríamos
                // buscar a "penúltima" compra (a última antes desta).
                // Por simplicidade, vamos usar o 'novoCustoMedio' ou 0.
                // Idealmente: _produtoDAO.BuscarCustoRealAnterior(itemCancelado.ProdutoId, compra.DataEmissao);
                decimal novoCustoUltimaCompra = (novaQtdTotal > 0) ? novoCustoMedio : 0;

                // D. Atualiza o Produto
                _produtoDAO.AtualizarEstoqueECusto(
                    itemCancelado.ProdutoId,
                    novaQtdTotal,
                    Math.Round(novoCustoMedio, 2), // Arredonda para 2 casas
                    Math.Round(novoCustoUltimaCompra, 2) // Arredonda para 2 casas
                );

                // E. Reverter ProdutoFornecedor (Lógica similar ao CustoUltimaCompra)
                // Idealmente: _produtoFornecedorDAO.ReverterUltimaCompra(itemCancelado.ProdutoId, fornecedorId, compra.DataEmissao);
                // Por enquanto, não faremos esta reversão para simplificar.
            }

            // --- PASSO 4: EXCLUIR PARCELAS "A PAGAR" ---
            _contaAPagarDAO.CancelarPorCompra(modelo, serie, numeroNota, fornecedorId, motivoCancelamento);

            // --- PASSO 5: MARCAR A COMPRA COMO CANCELADA ---
            // (Não excluímos o registro por motivos de auditoria)
            using (var conexao = new MySqlConnection(connectionString))
            {
                conexao.Open();
                string sql = @"
                    UPDATE Compra 
                    SET Status = false,  -- Define como Inativo/Cancelado
                    MotivoCancelamento = @MotivoCancelamento
                      WHERE Modelo = @Modelo 
                      AND Serie = @Serie 
                      AND NumeroNota = @NumeroNota 
                      AND FornecedorId = @FornecedorId";

                var cmd = new MySqlCommand(sql, conexao);

                cmd.Parameters.AddWithValue("@MotivoCancelamento", motivoCancelamento);

                cmd.Parameters.AddWithValue("@Modelo", modelo);
                cmd.Parameters.AddWithValue("@Serie", serie);
                cmd.Parameters.AddWithValue("@NumeroNota", numeroNota);
                cmd.Parameters.AddWithValue("@FornecedorId", fornecedorId);

                cmd.ExecuteNonQuery();
            }
        }
    }
}