package com.example.amfootball.ui.components.lists

import android.net.Uri
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AccountCircle
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.vector.rememberVectorPainter
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil.compose.AsyncImage

@Composable
fun ProfilesImage(
    image: Uri,
    contentDescription: String? = "",
    modifier: Modifier = Modifier
) {
    Box(
        modifier = modifier,
        contentAlignment = Alignment.Center
    ) {
        ImageIcon(
            contentDescription = contentDescription,
            image = image,
            sizeIamge = 100.dp,
        )
    }
}

@Composable
fun ImageList(
    image: Uri?
) {
    if (image != null && image != Uri.EMPTY) {
        ImageIcon(
            image = image,
            contentDescription = "Foto do jogador",
            sizeIamge = 40.dp
        )
    } else {
        Icon(
            imageVector = Icons.Default.AccountCircle,
            contentDescription = "Foto do jogador",
            modifier = Modifier.size(40.dp),
            tint = MaterialTheme.colorScheme.onSurfaceVariant
        )
    }
}

@Composable
fun ImageIcon(
    contentDescription: String? = "",
    image: Uri,
    sizeIamge: Dp
) {
    val fallbackPainter = rememberVectorPainter(Icons.Default.AccountCircle)

    AsyncImage(
        model = image,
        contentDescription = contentDescription,
        contentScale = ContentScale.Crop,
        fallback = fallbackPainter,
        error = fallbackPainter,
        modifier = Modifier
            .size(sizeIamge)
            .clip(CircleShape),
    )
}