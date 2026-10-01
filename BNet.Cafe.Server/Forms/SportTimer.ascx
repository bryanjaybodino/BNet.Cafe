<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="SportTimer.ascx.cs" Inherits="BNet.Cafe.Server.Forms.SportTimer" %>


<style>
/* Container */
.sport-banners-container {
    display: flex;
    flex-direction: column;
    gap: 0.875rem;
    width: 100%;
    box-sizing: border-box;
    margin: 1rem 0;
}

/* Banner Box */
.coming-soon-banner {
    position: relative;
    background-color: var(--bg-light-secondary);
    border: 1px solid var(--border-light);
    border-radius: 10px;
    padding: 1rem;
    box-sizing: border-box;
    overflow: hidden;
    transition: transform 0.2s ease, box-shadow 0.2s ease;
}

/* Sport Left Accents */
.banner-pickleball { border-left: 4px solid var(--accent-pickleball); }
.banner-billiard   { border-left: 4px solid var(--accent-billiard); }
.banner-pingpong   { border-left: 4px solid var(--accent-pingpong); }

/* Flexible Content Layout */
.banner-content {
    display: flex;
    align-items: flex-start;
    gap: 0.875rem;
    width: 100%;
}

/* Icon Box */
.banner-icon-wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 42px;
    height: 42px;
    border-radius: 8px;
    background-color: var(--bg-light-tertiary);
    flex-shrink: 0;
}

.banner-pickleball .banner-icon-wrapper { color: var(--accent-pickleball); }
.banner-billiard .banner-icon-wrapper   { color: var(--accent-billiard); }
.banner-pingpong .banner-icon-wrapper   { color: var(--accent-pingpong); }

.banner-icon {
    width: 24px;
    height: 24px;
}

/* Text Container */
.banner-text {
    flex: 1;
    min-width: 0; /* Prevents overflow in flexbox on small screens */
    padding-right: 5rem; /* Leaves room for the badge on desktop/mobile */
}

.banner-text h3 {
    margin: 0 0 0.25rem 0;
    font-size: 1rem;
    font-weight: 600;
    color: var(--text-light);
    line-height: 1.3;
}

.banner-text p {
    margin: 0;
    font-size: 0.8125rem;
    color: var(--text-light-secondary);
    line-height: 1.4;
    word-break: break-word;
}

/* Badge */
.banner-badge {
    position: absolute;
    top: 0.875rem;
    right: 0.875rem;
    background-color: var(--bg-light-tertiary);
    color: var(--primary);
    font-size: 0.6875rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    padding: 0.2rem 0.5rem;
    border-radius: 6px;
    border: 1px solid var(--border-light);
    white-space: nowrap;
}

/* Mobile Specific Optimizations (< 480px) */
@media (max-width: 480px) {
    .coming-soon-banner {
        padding: 0.875rem;
    }

    .banner-text {
        padding-right: 0; /* Remove right padding so text wraps under badge if needed */
    }

    .banner-text h3 {
        padding-right: 4.5rem; /* Keep title clear of badge */
        font-size: 0.95rem;
    }

    .banner-text p {
        font-size: 0.775rem;
        margin-top: 0.35rem;
    }

    .banner-icon-wrapper {
        width: 36px;
        height: 36px;
    }

    .banner-icon {
        width: 20px;
        height: 20px;
    }

    .banner-badge {
        top: 0.75rem;
        right: 0.75rem;
        font-size: 0.625rem;
        padding: 0.15rem 0.4rem;
    }
}
</style>
<!-- Coming Soon Banners Container -->
<div class="sport-banners-container">

    <!-- 1. Pickleball Timer Banner -->
    <div class="coming-soon-banner banner-pickleball">
        <span class="banner-badge">Coming Soon</span>
        <div class="banner-content">
            <div class="banner-icon-wrapper">
                <svg class="banner-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M10.5 3A6.5 6.5 0 0 0 4 9.5c0 2.8 1.8 5.2 4.3 6.1L7 21h3l1.8-5.3c.7.2 1.4.3 2.2.3a6.5 6.5 0 0 0 6.5-6.5A6.5 6.5 0 0 0 10.5 3z"></path>
                    <circle cx="10.5" cy="8.5" r="1" fill="currentColor"></circle>
                    <circle cx="13" cy="11" r="1" fill="currentColor"></circle>
                </svg>
            </div>
            <div class="banner-text">
                <h3>Pickleball Timer</h3>
                <p>Court booking management, automated score tracks, and match interval timing.</p>
            </div>
        </div>
    </div>

    <!-- 2. Billiard Timer Banner -->
    <div class="coming-soon-banner banner-billiard">
        <span class="banner-badge">Coming Soon</span>
        <div class="banner-content">
            <div class="banner-icon-wrapper">
                <svg class="banner-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <circle cx="12" cy="12" r="10"></circle>
                    <circle cx="12" cy="12" r="4" fill="var(--bg-light-secondary)"></circle>
                    <path d="M12 10a1 1 0 1 0 0 2 1 1 0 0 0 0-2zm0 2a1 1 0 1 0 0 2 1 1 0 0 0 0-2z" fill="var(--text-light)"></path>
                </svg>
            </div>
            <div class="banner-text">
                <h3>Billiard Timer</h3>
                <p>Automated table lights activation, hourly rate billing, and game session logs.</p>
            </div>
        </div>
    </div>

    <!-- 3. Ping Pong Timer Banner -->
    <div class="coming-soon-banner banner-pingpong">
        <span class="banner-badge">Coming Soon</span>
        <div class="banner-content">
            <div class="banner-icon-wrapper">
                <svg class="banner-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <path d="M14.5 2a5.5 5.5 0 0 0-5.5 5.5c0 1.9.9 3.6 2.4 4.6L8 20h3l3.2-7.2c.1 0 .2.1.3.1a5.5 5.5 0 0 0 5.5-5.5A5.5 5.5 0 0 0 14.5 2z"></path>
                    <circle cx="5" cy="18" r="2" fill="currentColor"></circle>
                </svg>
            </div>
            <div class="banner-text">
                <h3>Ping Pong Timer</h3>
                <p>Fast-paced table rotation timing, quick set reset, and tournament clock mode.</p>
            </div>
        </div>
    </div>

</div>