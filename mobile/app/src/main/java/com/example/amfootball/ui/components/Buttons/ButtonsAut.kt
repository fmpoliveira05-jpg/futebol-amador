package com.example.amfootball.ui.components.Buttons

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.example.amfootball.ui.theme.FacebookBlue
import com.example.amfootball.ui.theme.FacebookWhite
import com.example.amfootball.ui.theme.GoogleWhite
import com.example.amfootball.R

/*
@Composable
fun GoogleSignInButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    isLoading: Boolean = false
) {
    Button(
        onClick = onClick,
        modifier = modifier.fillMaxWidth(),
        enabled = !isLoading,
        colors = ButtonDefaults.buttonColors(
            containerColor = GoogleWhite,
            contentColor = Color.Black // Cor do texto
        ),
        elevation = ButtonDefaults.buttonElevation(defaultElevation = 2.dp)
    ) {
        Row(
            modifier = Modifier.padding(vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.Center
        ) {
            Image(
                painter = painterResource(id = R.drawable.ic_google_logo), // O teu ícone
                contentDescription = "Google Logo",
                modifier = Modifier.size(24.dp)
            )
            Text(
                text = if (isLoading) "A iniciar sessão..." else "Continuar com o Google",
                modifier = Modifier.padding(start = 16.dp),
                fontWeight = FontWeight.Bold,
                color = Color.Gray // O texto do Google não é preto puro
            )
        }
    }
}

@Composable
fun FacebookSignInButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    isLoading: Boolean = false
) {
    Button(
        onClick = onClick,
        modifier = modifier.fillMaxWidth(),
        enabled = !isLoading,
        colors = ButtonDefaults.buttonColors(
            containerColor = FacebookBlue,
            contentColor = FacebookWhite
        )
    ) {
        Row(
            modifier = Modifier.padding(vertical = 8.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.Center
        ) {
            Image(
                painter = painterResource(id = R.drawable.ic_facebook_logo), // O teu ícone
                contentDescription = "Facebook Logo",
                modifier = Modifier.size(24.dp)
            )
            Text(
                text = if (isLoading) "A iniciar sessão..." else "Continuar com o Facebook",
                modifier = Modifier.padding(start = 16.dp),
                fontWeight = FontWeight.Bold
            )
        }
    }
}

@Composable
private fun ButtonAut(onClick: () -> Unit,
                      modifier: Modifier = Modifier,
                      isLoading: Boolean = false,
                      containerColor: Color,
                      contentColor: Color,
                      logo:
) {

}
* */