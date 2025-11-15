package com.example.amfootball.data.dtos.player

import android.net.Uri
import com.example.amfootball.data.enums.Position
import java.util.UUID

data class InfoPlayerDto(
    val id: String = "",
    val name: String = "",
    val image: Uri = Uri.EMPTY,
    val address: String = "",
    val age: Int = 0,
    val position: Position,
    val size: Int = 0,
) {
    companion object {
        fun createExamplePlayerList(): List<InfoPlayerDto> {
            val firstNames = listOf(
                "Diogo", "Bruno", "João", "Rui", "Cristiano", "Pedro",
                "Miguel", "Nuno", "André", "Fábio"
            )
            val lastNames = listOf(
                "Silva", "Costa", "Fernandes", "Santos", "Gomes",
                "Lopes", "Alves", "Martins", "Dias", "Ribeiro"
            )

            val cities = listOf("Lisboa", "Porto", "Braga", "Faro", "Coimbra", "Aveiro")

            val allPositions = Position.values()

            return (1..20).map {
                InfoPlayerDto(
                    id = UUID.randomUUID().toString(),
                    name = "${firstNames.random()} ${lastNames.random()}",
                    address = cities.random(),
                    age = (18..38).random(),
                    position = allPositions.random(),
                    size = (165..195).random()
                )
            }
        }
    }
}
