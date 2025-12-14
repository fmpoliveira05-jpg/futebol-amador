package com.example.amfootball.data.remote.dtos

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa um resultado de localização (Place)
 * retornado pela API de Geocodificação Nominatim do OpenStreetMap.
 *
 * Este objeto é usado para armazenar as coordenadas geográficas e informações textuais
 * de um endereço ou local pesquisado.
 *
 * @property placeId O identificador único do local (Place ID) no banco de dados do OpenStreetMap.
 * @property displayName O nome completo e formatado do local (inclui rua, cidade, país, etc.),
 * ideal para ser exibido na UI.
 * @property lat A latitude do local, fornecida como uma string (deve ser convertida para Double, se necessário,
 * para operações geográficas).
 * @property lon A longitude do local, fornecida como uma string (deve ser convertida para Double, se necessário,
 * para operações geográficas).
 */
data class OpenStreetMapPlace(
    @SerializedName("place_id") val placeId: Long,
    @SerializedName("display_name") val displayName: String,
    @SerializedName("lat") val lat: String,
    @SerializedName("lon") val lon: String
)