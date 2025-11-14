package com.example.amfootball.data.dtos.player

import android.net.Uri
import java.time.LocalDate

data class PlayerProfileDto(
    val name: String = "",
    val dateOfBirthday: LocalDate? = null,
    val icon: Uri = Uri.EMPTY,
    val address: String = "",
    val phoneNumber: String = "",
    val email: String = "",
    val position: String = "",
    val size: Int = 0,
    val team: String = ""
) {
    companion object {
        fun createExample(): PlayerProfileDto {
            return PlayerProfileDto(
                name = "Jogador Exemplo",
                dateOfBirthday = LocalDate.of(1995, 10, 20),
                icon = Uri.EMPTY,
                address = "Rua da Simulação, Guimarães",
                phoneNumber = "912345678",
                email = "jogador.exemplo@email.com",
                position = "Avançado",
                size = 180,
                team = "Os Invencíveis FC"
            )
        }
    }
}
