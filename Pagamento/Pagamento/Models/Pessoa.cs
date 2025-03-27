namespace Pagamento.Models
{
    public class Pessoa
    {
        public int IdPessoa { get; set; }
        public string TipoPessoa { get; set; } 
        public string Nome_RazaoSocial { get; set; }
        public DateTime DataNascimento_Fundacao { get; set; }
        public string CPF_CNPJ { get; set; }
        public string RG_InsMunicipal { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Endereco { get; set; }
        public string Bairro { get; set; }
        public string Cep { get; set; }
        public string Status { get; set; } 
        public int IdCidade { get; set; }
    }
}
