namespace Application.Interfaces.Repositories
{
    /// <summary>
    /// Define o contrato para a implementação do padrão Unit of Work (Unidade de Trabalho).
    /// 
    /// Esta interface é utilizada para agrupar múltiplas operações de repositório (adições, atualizações, remoções)
    /// numa única transação atómica, garantindo que todas as mudanças são persistidas (commit) ou nenhuma delas o é (rollback).
    /// </summary>
    public interface IUnityOfWork
    {

        /// <summary>
        /// Persiste de forma assíncrona todas as alterações rastreadas (tracked changes) na base de dados.
        /// </summary>
        /// <remarks>
        /// Este método é o ponto de chamada final para a transação. O valor de retorno indica o número de registos afetados.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Valor de Retorno</term>
        ///         <description>Significado</description>
        ///     </listheader>
        ///     <item>
        ///         <term>0</term>
        ///         <description>Nenhuma alteração foi efetuada.</description>
        ///     </item>
        ///     <item>
        ///         <term>Maior que 0</term>
        ///         <description>O número de entidades (linhas) que foram adicionadas, atualizadas ou eliminadas.</description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <returns>Uma tarefa assíncrona (<see cref="Task"/>) que retorna o número de entradas de estado escritas na base de dados.</returns>
        public Task<int> SaveChangesAsync();
    }
}