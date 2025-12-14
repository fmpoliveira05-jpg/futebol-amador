package com.example.amfootball.data.interfaces.services

import com.example.amfootball.data.remote.dtos.OpenStreetMapPlace
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query

/**
 * Interface de serviço para comunicação com a API Nominatim do OpenStreetMap.
 *
 * Esta interface é tipicamente usada em conjunto com o Retrofit para realizar
 * operações de **geocodificação** (converter um endereço textual numa coordenada geográfica)
 * e **geocodificação inversa** (converter coordenadas em endereço), embora atualmente
 * apenas suporte a pesquisa de endereço (`search`).
 *
 * URL Base Esperada (Nominatim): `https://nominatim.openstreetmap.org/`
 */
interface OpenStreetMapService {

    /**
     * Realiza uma pesquisa de endereço (geocodificação) usando o endpoint `/search` do Nominatim.
     *
     * Este método pesquisa uma string de endereço e devolve uma lista de resultados
     * que correspondem ao endereço fornecido.
     *
     * @param address O endereço ou nome do local a ser pesquisado.
     * @param format O formato de saída dos dados. Por predefinição, é "json".
     * @param limit O número máximo de resultados a serem devolvidos. Por predefinição, é 1
     * (útil para obter a correspondência mais provável).
     * @param userAgent Cabeçalho "User-Agent" obrigatório pela política de uso do Nominatim.
     * Identifica a aplicação que está a aceder ao serviço.
     * @return Uma lista de objetos [OpenStreetMapPlace] contendo os detalhes do local
     * encontrado (incluindo latitude e longitude).
     */
    @GET("search")
    suspend fun verifyAddress(
        @Query("q") address: String,
        @Query("format") format: String = "json",
        @Query("limit") limit: Int = 1,
        @Header("User-Agent") userAgent: String = "AMFootball-App (willkie79@.com)"
    ): List<OpenStreetMapPlace>
}