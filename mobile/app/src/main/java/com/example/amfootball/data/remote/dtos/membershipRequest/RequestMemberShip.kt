package com.example.amfootball.data.remote.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) utilizado para realizar operações que requerem a identificação
 * de um pedido de adesão (membership request) específico, tipicamente para aceitação ou rejeição.
 *
 * Esta classe transporta o identificador único de um pedido de adesão para a API.
 *
 * @property requestId O identificador único do pedido de adesão. Mapeado a partir do campo JSON "RequestId".
 */
data class RequestMemberShip(
    @SerializedName("RequestId")
    val requestId: String
)
