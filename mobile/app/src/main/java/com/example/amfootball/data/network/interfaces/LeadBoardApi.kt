package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.leadboard.InfoTeamLeadboard
import retrofit2.Response
import retrofit2.http.GET

/**
 * Interface de definição dos endpoints da API Retrofit relacionados com a Tabela de Classificação (Leaderboard).
 *
 * Esta interface mapeia as chamadas HTTP necessárias para consultar os rankings e pontuações
 * das equipas registadas na plataforma.
 */
interface LeadBoardApi {

    /**
     * Obtém os dados completos da tabela de classificação atual.
     *
     * Executa um pedido **GET** para o endpoint definido em [BaseEndpoints.LEADBOARD_API].
     *
     * @return Um objeto [Response] do Retrofit contendo o DTO [InfoTeamLeadboard].
     * Este DTO encapsula a lista de equipas ordenadas e eventuais metadados de paginação ou época.
     */
    @GET(BaseEndpoints.LEADBOARD_API)
    suspend fun getLeadBoard(): Response<InfoTeamLeadboard>
}