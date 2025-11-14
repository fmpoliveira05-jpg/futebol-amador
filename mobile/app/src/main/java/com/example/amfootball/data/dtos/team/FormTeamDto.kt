package com.example.amfootball.data.dtos.team

import android.net.Uri
import com.example.amfootball.data.PitchFormDto

data class FormTeamDto(
    val id: String? = null,
    val name: String = "",
    val description: String? = null,
    val image: Uri? = null,
    val pitch: PitchFormDto = PitchFormDto()
) {
    companion object {
        fun generateEditExampleTeam(): FormTeamDto {
            return FormTeamDto(
                id = "Team1",
                name = "Team Edit",
                description = "Descrição da equipa",
                image = null,
                pitch = PitchFormDto(
                    id = "campo 1",
                    name = "Campo da equipa",
                    address = "Rua do campo, Guimarães"
                )
            )
        }
    }
}