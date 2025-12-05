package com.example.amfootball.data.dtos.support

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa as informações de nome e endereço de um campo de jogo.
 *
 * O uso de [SerializedName] com [alternate] garante a flexibilidade ao desserializar
 * os dados vindos do backend (ex: se as chaves JSON variarem entre "Name" e "name").
 *
 * @property name O nome do campo ou estádio. Configurado para aceitar "Name" ou "name" no JSON.
 * @property address O endereço físico do campo. Configurado para aceitar "Address" ou "address" no JSON.
 */
data class PitchInfo(
    @SerializedName("Name", alternate = ["name"])
    val name: String = "",
    @SerializedName("Address", alternate = ["address"])
    val address: String = ""
)