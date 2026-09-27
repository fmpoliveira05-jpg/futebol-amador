package com.example.amfootball.data.remote.dtos.player

import com.google.gson.annotations.SerializedName

/** Pedido de eliminação da conta: a palavra-passe atual confirma que é o titular. */
data class EliminarContaDto(
    @SerializedName("password")
    val password: String
)
