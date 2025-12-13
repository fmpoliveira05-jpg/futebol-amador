package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.filters.FilterMatchInvite
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.interfaces.api.MatchInviteApi
import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.matchInivite.SendMatchInviteDto
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável pela gestão do ciclo de vida de convites de jogo (Match Invites).
 *
 * Esta classe atua como camada de repositório para operações de desafio entre equipas.
 * Utiliza o utilitário [safeApiCallWithReturn] para abstrair o tratamento de erros HTTP e
 * garantir a consistência dos dados recebidos da [MatchInviteApi].
 *
 * @property matchInviteApi A interface Retrofit injetada para comunicação com o backend.
 */
@Singleton
class MatchInviteService @Inject constructor(
    private val matchInviteApi: MatchInviteApi
) {

    /**
     * Envia um novo convite de jogo (Desafio) para uma equipa adversária.
     *
     * Delega a execução para [safeApiCallWithReturn], que executa a chamada de rede,
     * valida se a resposta foi bem-sucedida e se o corpo não é nulo.
     *
     * @param teamId O identificador da equipa que está a enviar o convite (Desafiante).
     * @param matchInvite O DTO contendo os detalhes do jogo proposto (Data, Hora, Local, Adversário).
     * @return [MatchInviteDto] contendo os dados do convite recém-criado e o seu estado inicial (ex: PENDING).
     * @throws Exception Propagada automaticamente se ocorrer erro de rede ou se a API devolver um código de erro (4xx/5xx).
     */
    suspend fun sendMatchInvite(teamId: String, matchInvite: SendMatchInviteDto): MatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.sendMatchInvite(idTeam = teamId, dto = matchInvite)
        }
    }

    /**
     * Obtém uma lista de convites de jogo associados a uma equipa, aplicando filtros opcionais.
     *
     * Os filtros são convertidos para um mapa de query parameters através da extensão [toQueryMap].
     *
     * @param teamId O identificador da equipa para a qual os convites estão a ser solicitados.
     * @param filter O objeto [FilterMatchInvite] contendo os critérios de filtragem (ex: estado do convite).
     * @return [List] de [MatchInviteDto] que correspondem aos critérios de filtro.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação.
     */
    suspend fun getListMatchInvite(teamId: String, filter: FilterMatchInvite): List<MatchInviteDto> {

        return safeApiCallWithReturn {
            val filterMatchInvite = filter.toQueryMap() ?: emptyMap()
            matchInviteApi.getMatchInviteList(idTeam = teamId, filterMatchInvite)
        }
    }

    /**
     * Rejeita um convite de jogo específico.
     *
     * Esta operação não retorna dados (Unit), utilizando [safeApiCallWithNotReturn] para
     * garantir que a chamada foi executada com sucesso.
     *
     * @param teamId O identificador da equipa que está a rejeitar o convite.
     * @param matchInviteId O identificador único do convite a ser rejeitado.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação.
     */
    suspend fun rejectMatchInivite(teamId: String, matchInviteId: String) {
        safeApiCallWithNotReturn {
            matchInviteApi.refuseMatchInvite(idTeam = teamId, matchInviteId = matchInviteId)
        }
    }

    /**
     * Aceita um convite de jogo recebido.
     *
     * @param teamId O identificador da equipa que está a aceitar o convite.
     * @param matchInviteId O identificador único do convite a ser aceite.
     * @return [SendMatchInviteDto] contendo os detalhes do jogo confirmado, ou um DTO relevante para a resposta de aceitação.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação.
     */
    suspend fun acceptMatchInvitee(teamId: String, matchInviteId: String): SendMatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.acceptMatchInvite(idTeam = teamId, matchInviteId = matchInviteId)
        }
    }

    /**
     * Envia uma contra-proposta (Negociação) para um convite recebido.
     *
     * Utilizado quando uma equipa tem interesse no jogo, mas pretende alterar condições específicas
     * (ex: propor uma nova hora ou mudar o campo) em vez de aceitar ou rejeitar imediatamente.
     *
     * @param teamId O identificador da equipa que está a realizar a negociação.
     * @param matchInvite O DTO com os novos detalhes propostos para o jogo.
     * @return [MatchInviteDto] com o convite atualizado refletindo a negociação.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação ou validação.
     */
    suspend fun negociateMatchInvite(
        teamId: String,
        matchInvite: SendMatchInviteDto
    ): MatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.negotiateMatch(idTeam = teamId, matchInvite = matchInvite)
        }
    }

    /**
     * Obtém os detalhes de um convite de jogo específico pelo seu ID.
     *
     * @param teamId O identificador da equipa associada à busca (contexto de segurança/permissão).
     * @param matchInviteId O identificador único do convite a ser obtido.
     * @return [MatchInviteDto] contendo todos os detalhes do convite de jogo.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação ou se o recurso não for encontrado.
     */
    suspend fun getInviteMatch(teamId: String, matchInviteId: String): MatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.getMatchInvite(idTeam = teamId, idMatchInvite = matchInviteId)
        }
    }
}