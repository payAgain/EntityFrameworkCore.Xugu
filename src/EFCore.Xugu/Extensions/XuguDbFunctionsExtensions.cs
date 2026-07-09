using System;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     XuguDB DbFunctions extensions (DateDiff via TIMESTAMPDIFF, Like).
///     Docs: reference/function/date-and-time-functions/timestampdiff.md
/// </summary>
public static class XuguDbFunctionsExtensions
{
        #region DateDiffYear

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int DateDiffYear(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        /// <summary>
        ///     Counts the number of year boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(YEAR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of year boundaries crossed between the dates.</returns>
        public static int? DateDiffYear(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffYear)));

        #endregion DateDiffYear

        #region DateDiffQuarter

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int DateDiffQuarter(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        /// <summary>
        ///     Counts the number of quarter boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(QUARTER,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of quarter boundaries crossed between the dates.</returns>
        public static int? DateDiffQuarter(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffQuarter)));

        #endregion DateDiffQuarter

        #region DateDiffMonth

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int DateDiffMonth(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        /// <summary>
        ///     Counts the number of month boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MONTH,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of month boundaries crossed between the dates.</returns>
        public static int? DateDiffMonth(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMonth)));

        #endregion DateDiffMonth

        #region DateDiffWeek

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int DateDiffWeek(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        /// <summary>
        ///     Counts the number of week boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(WEEK,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of week boundaries crossed between the dates.</returns>
        public static int? DateDiffWeek(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffWeek)));

        #endregion DateDiffWeek

        #region DateDiffDay

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int DateDiffDay(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        /// <summary>
        ///     Counts the number of day boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(DAY,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of day boundaries crossed between the dates.</returns>
        public static int? DateDiffDay(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffDay)));

        #endregion DateDiffDay

        #region DateDiffHour

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int DateDiffHour(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        /// <summary>
        ///     Counts the number of hour boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(HOUR,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of hour boundaries crossed between the dates.</returns>
        public static int? DateDiffHour(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffHour)));

        #endregion DateDiffHour

        #region DateDiffMinute

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int DateDiffMinute(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        /// <summary>
        ///     Counts the number of minute boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MINUTE,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of minute boundaries crossed between the dates.</returns>
        public static int? DateDiffMinute(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMinute)));

        #endregion DateDiffMinute

        #region DateDiffSecond

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int DateDiffSecond(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        /// <summary>
        ///     Counts the number of second boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(SECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of second boundaries crossed between the dates.</returns>
        public static int? DateDiffSecond(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffSecond)));

        #endregion DateDiffSecond

        #region DateDiffMillisecond

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int DateDiffMillisecond(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        /// <summary>
        ///     Counts the number of millisecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) DIV 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of millisecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMillisecond(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMillisecond)));

        #endregion DateDiffMillisecond

        #region DateDiffMicrosecond

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int DateDiffMicrosecond(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        /// <summary>
        ///     Counts the number of microsecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate)`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of microsecond boundaries crossed between the dates.</returns>
        public static int? DateDiffMicrosecond(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffMicrosecond)));

        #endregion DateDiffMicrosecond

        #region DateDiffTick

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int DateDiffTick(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        /// <summary>
        ///     Counts the number of tick boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 10`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of tick boundaries crossed between the dates.</returns>
        public static int? DateDiffTick(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffTick)));

        #endregion DateDiffTick

        #region DateDiffNanosecond

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            DateTime startDate,
            DateTime endDate)
         => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            DateTime? startDate,
            DateTime? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            DateTimeOffset startDate,
            DateTimeOffset endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            DateTimeOffset? startDate,
            DateTimeOffset? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int DateDiffNanosecond(
            this DbFunctions _,
            DateOnly startDate,
            DateOnly endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        /// <summary>
        ///     Counts the number of nanosecond boundaries crossed between the startDate and endDate.
        ///     Corresponds to `TIMESTAMPDIFF(MICROSECOND,startDate,endDate) * 1000`.
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="startDate">Starting date for the calculation.</param>
        /// <param name="endDate">Ending date for the calculation.</param>
        /// <returns>Number of nanosecond boundaries crossed between the dates.</returns>
        public static int? DateDiffNanosecond(
            this DbFunctions _,
            DateOnly? startDate,
            DateOnly? endDate)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(DateDiffNanosecond)));

        #endregion DateDiffNanosecond

        #region Like

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive. If this function is evaluated on the client, then it will always use
        ///         a case-insensitive comparison.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The property of entity that is to be matched.</param>
        /// <param name="pattern">
        ///     The pattern which may involve the wildcards `%` and `_`. The character `\` is used to escape wildcards and itself.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like<T>(
            this DbFunctions _,
            T matchExpression,
            string pattern)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        /// <summary>
        ///     <para>
        ///         An implementation of the SQL LIKE operation. On relational databases this is usually directly
        ///         translated to SQL.
        ///     </para>
        ///     <para>
        ///         Note that if this function is translated into SQL, then the semantics of the comparison will
        ///         depend on the database configuration. In particular, it may be either case-sensitive or
        ///         case-insensitive.
        ///     </para>
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="matchExpression">The property of entity that is to be matched.</param>
        /// <param name="pattern">The pattern which may involve the wildcards `%` and `_`.</param>
        /// <param name="escapeCharacter">
        ///     The escape character (as a single character string) to use in front of `%` and `_` (if they are not used as wildcards), and
        ///     itself.
        /// </param>
        /// <returns>true if there is a match.</returns>
        public static bool Like<T>(
            this DbFunctions _,
            T matchExpression,
            string pattern,
            string escapeCharacter)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Like)));

        #endregion Like

        #region Hex

        /// <summary>
        ///     Converts a string or numeric value to a hexadecimal string via <c>HEX(expr)</c>.
        ///     Docs: reference/function/uncategorized-functions/hex.md
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The string or number to convert.</param>
        /// <returns>The hexadecimal string, or <see langword="null" />.</returns>
        public static string Hex<T>(
            this DbFunctions _,
            T value)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Hex)));

        #endregion Hex

        #region Unhex

        /// <summary>
        ///     Converts a hexadecimal string to its ASCII representation via <c>UNHEX(expr)</c>.
        ///     Docs: reference/function/uncategorized-functions/unhex.md
        /// </summary>
        /// <param name="_">The DbFunctions instance.</param>
        /// <param name="value">The hexadecimal string to convert.</param>
        /// <returns>The decoded string, or <see langword="null" />.</returns>
        public static string Unhex(
            this DbFunctions _,
            string value)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Unhex)));

        #endregion Unhex

        #region Degrees

        /// <summary>
        ///     Converts radians to degrees. Corresponds to <c>DEGREES(radians)</c>.
        /// </summary>
        [DbFunction(name: "DEGREES", IsBuiltIn = true)]
        public static double Degrees(
            this DbFunctions _,
            double radians)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Degrees)));

        /// <summary>
        ///     Converts radians to degrees. Corresponds to <c>DEGREES(radians)</c>.
        /// </summary>
        [DbFunction(name: "DEGREES", IsBuiltIn = true)]
        public static float Degrees(
            this DbFunctions _,
            float radians)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Degrees)));

        #endregion Degrees

        #region Radians

        /// <summary>
        ///     Converts degrees to radians. Corresponds to <c>RADIANS(degrees)</c>.
        /// </summary>
        [DbFunction(name: "RADIANS", IsBuiltIn = true)]
        public static double Radians(
            this DbFunctions _,
            double degrees)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Radians)));

        /// <summary>
        ///     Converts degrees to radians. Corresponds to <c>RADIANS(degrees)</c>.
        /// </summary>
        [DbFunction(name: "RADIANS", IsBuiltIn = true)]
        public static float Radians(
            this DbFunctions _,
            float degrees)
            => throw new InvalidOperationException(CoreStrings.FunctionOnClient(nameof(Radians)));

        #endregion Radians
}
