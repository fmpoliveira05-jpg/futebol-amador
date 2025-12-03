package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.network.interfaces.MatchInviteApi
import com.example.amfootball.utils.safeApiCallWithReturn
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
     * @return [InfoMatchInviteDto] contendo os dados do convite recém-criado e o seu estado inicial (ex: PENDING).
     * @throws Exception Propagada automaticamente se ocorrer erro de rede ou se a API devolver um código de erro (4xx/5xx).
     */
    suspend fun sendMatchInvite(
        teamId: String,
        matchInvite: SendMatchInviteDto
    ): InfoMatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.sendMatchInvite(idTeam = teamId, matchInivite = matchInvite)
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
     * @return [InfoMatchInviteDto] com o convite atualizado refletindo a negociação.
     * @throws Exception Propagada automaticamente em caso de falha na comunicação ou validação.
     */
    suspend fun negociateMatchInvite(
        teamId: String,
        matchInvite: SendMatchInviteDto
    ): InfoMatchInviteDto {
        return safeApiCallWithReturn {
            matchInviteApi.negotiateMatch(idTeam = teamId, matchInivite = matchInvite)
        }
    }
}