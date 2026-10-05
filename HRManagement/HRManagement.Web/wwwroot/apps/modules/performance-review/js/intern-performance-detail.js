const API_BASE = "https://localhost:7089";

let planId = null;
let internId = null;
let performanceData = null;

document.addEventListener("DOMContentLoaded", initializePage);

async function initializePage() {
    try {
        const params = new URLSearchParams(window.location.search);

        planId = Number(params.get("planId"));
        internId = Number(params.get("internId"));

        if (!planId || !internId) {
            renderError("Invalid performance detail parameters.");
            return;
        }

        await loadPerformanceDetail();
    } catch (error) {
        console.error("Failed to initialize performance detail page:", error);
        renderError("Failed to load intern performance details.");
    }
}

function getToken() {
    return window.aiaAuth ? window.aiaAuth.getToken() : null;
}

async function apiGet(url) {
    const token = getToken();

    if (!token) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Authentication token not found.");
    }

    const response = await fetch(`${API_BASE}${url}`, {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": `Bearer ${token}`
        }
    });

    if (response.status === 401) {
        if (window.aiaAuth) window.aiaAuth.signOut();
        throw new Error("Unauthorized.");
    }

    if (!response.ok) throw new Error(`API request failed: ${response.status}`);

    return await response.json();
}

async function loadPerformanceDetail() {
    const result = await apiGet(`/api/intern-performance/${planId}/${internId}/detail`);
    performanceData = result.content ?? result;

    if (!performanceData) {
        renderError("No performance details are available.");
        return;
    }

    renderHeader();
    renderSummary();
    renderTrend();
    renderPeriodBreakdown();
}

function renderHeader() {
    document.getElementById("internName").textContent = performanceData.fullName ?? "-";
    document.getElementById("internRole").textContent = performanceData.internRole ?? "-";
}

function renderSummary() {
    const peak = performanceData.peakPerformanceScore;
    const lowest = performanceData.lowestPerformanceScore;
    const annualAverage = performanceData.annualAverageScore;

    document.getElementById("peakScore").textContent = formatScoreValue(peak?.overallScore);
    document.getElementById("peakPeriod").textContent = peak?.periodLabel ?? "-";

    document.getElementById("lowestScore").textContent = formatScoreValue(lowest?.overallScore);
    document.getElementById("lowestPeriod").textContent = lowest?.periodLabel ?? "-";

    document.getElementById("annualAverage").textContent = formatScoreValue(annualAverage);
}

function renderTrend() {
    const periods = performanceData.periods ?? [];

    document.getElementById("chartLoading").classList.add("d-none");

    if (!periods.length) {
        document.getElementById("chartEmpty").classList.remove("d-none");
        return;
    }

    document.getElementById("performanceChart").classList.remove("d-none");

    const periodType = performanceData.periodType ?? "Period";
    document.getElementById("trendDescription").textContent = `${periodType} performance trajectory`;

    const svg = document.getElementById("trendSvg");

    const width = 1000;
    const height = 280;
    const paddingX = 35;
    const paddingTop = 25;
    const paddingBottom = 45;

    const values = periods.map(period => Number(period.overallScore ?? 0));
    const minValue = Math.min(...values);
    const maxValue = Math.max(...values);
    const range = maxValue - minValue || 1;

    const chartWidth = width - paddingX * 2;
    const chartHeight = height - paddingTop - paddingBottom;

    const points = periods.map((period, index) => {
        const x = periods.length === 1
            ? width / 2
            : paddingX + (index / (periods.length - 1)) * chartWidth;

        const y = paddingTop + ((maxValue - Number(period.overallScore ?? 0)) / range) * chartHeight;

        return { x, y, label: period.periodLabel, value: period.overallScore };
    });

    const path = points.map((point, index) => `${index === 0 ? "M" : "L"} ${point.x} ${point.y}`).join(" ");

    let svgContent = `
                <line x1="${paddingX}" y1="${height - paddingBottom}" x2="${width - paddingX}" y2="${height - paddingBottom}" stroke="#E4E6EF" stroke-width="1" />
                <path d="${path}" class="intern-performance-chart-line" />
            `;

    points.forEach(point => {
        svgContent += `
                    <circle cx="${point.x}"
                    cy="${point.y}"
                    r="5"
                    class="intern-performance-chart-point" />
                    <text x="${point.x}" y="${point.y - 12}" text-anchor="middle" fill="#5E6278" font-size="14" font-weight="600">${formatScoreValue(point.value)}</text>
                    <text x="${point.x}" y="${height - 12}" text-anchor="middle" fill="#7E8299" font-size="12">${escapeHtml(point.label)}</text>
                `;
    });

    svg.innerHTML = svgContent;
}

function renderPeriodBreakdown() {
    const periods = performanceData.periods ?? [];
    const tbody = document.getElementById("periodTableBody");
    const title = document.getElementById("breakdownTitle");

    const periodType = performanceData.periodType ?? "Period";

    title.textContent = `${periodType} Performance Breakdown`;

    if (!periods.length) {
        tbody.innerHTML = `
                    <tr>
                        <td colspan="6" class="text-center py-15">
                            <i class="bi bi-bar-chart-line fs-2x text-gray-400 d-block mb-3"></i>
                            <div class="fw-bold text-gray-700 fs-6 mb-1">No Performance Data</div>
                            <div class="text-muted fs-7">No period performance data is available.</div>
                        </td>
                    </tr>`;
        return;
    }

    tbody.innerHTML = periods.map(period => `
                <tr>
                    <td class="ps-6 ps-lg-8">
                        <span class="fw-semibold text-gray-800 fs-7">${escapeHtml(period.periodLabel)}</span>
                    </td>

                    <td class="text-center text-gray-600 fs-7">
                        ${formatScore(period.techScore)}
                    </td>

                    <td class="text-center text-gray-600 fs-7">
                        ${formatScore(period.softSkillScore)}
                    </td>

                    <td class="text-center text-gray-600 fs-7">
                        ${formatScore(period.selfAssessmentScore)}
                    </td>

                    <td class="text-center text-gray-600 fs-7">
                        ${formatScore(period.peerReviewScore)}
                    </td>

                    <td class="text-center pe-6 pe-lg-8">
                        <span class="fw-bold text-gray-800 fs-7">
                            ${formatScore(period.overallScore)}
                        </span>
                    </td>
                </tr>
            `).join("");
}

function formatScore(value) {
    return value === null || value === undefined || value === "" ? "-" : formatScoreValue(value);
}

function formatScoreValue(value) {
    return value === null || value === undefined || value === "" ? "-" : Number(value).toFixed(2);
}

function renderError(message) {
    document.getElementById("internName").textContent = "Unable to Load Performance";
    document.getElementById("internRole").textContent = message;

    document.getElementById("chartLoading").classList.add("d-none");
    document.getElementById("chartEmpty").classList.remove("d-none");

    document.getElementById("periodTableBody").innerHTML = `
                <tr>
                    <td colspan="6" class="text-center py-15">
                        <i class="bi bi-exclamation-circle fs-2x text-danger d-block mb-3"></i>
                        <div class="fw-bold text-gray-700 fs-6 mb-1">Something went wrong</div>
                        <div class="text-muted fs-7">${escapeHtml(message)}</div>
                    </td>
                </tr>`;
}

function escapeHtml(value) {
    if (value === null || value === undefined) return "";

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}