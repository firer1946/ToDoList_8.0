
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "TDCEnumContainers.h"
#include "TDCCustomAttributeHelper.h"

#include "..\shared\Misc.h"

#include "..\Interfaces\Preferences.h"

//////////////////////////////////////////////////////////////////////

CTDCAttributeMap::CTDCAttributeMap() 
	: CTDCBaseEnumSet<TDC_ATTRIBUTE>()
{
}

CTDCAttributeMap::CTDCAttributeMap(TDC_ATTRIBUTE nAttrib)
	: CTDCBaseEnumSet<TDC_ATTRIBUTE>()
{
	Add(nAttrib);
}

CTDCAttributeMap::CTDCAttributeMap(const CTDCAttributeMap& mapOther) 
	: CTDCBaseEnumSet<TDC_ATTRIBUTE>(mapOther)
{
}

CTDCAttributeMap::~CTDCAttributeMap()
{
}

BOOL CTDCAttributeMap::Add(TDC_ATTRIBUTE nAttrib)
{
	if (!CanAdd(nAttrib))
		return FALSE;

	CTDCBaseEnumSet<TDC_ATTRIBUTE>::Add(nAttrib);
	return TRUE;
}


int CTDCAttributeMap::Append(const CTDCAttributeMap& other)
{
	POSITION pos = other.GetStartPosition();
	int nAdded = 0;

	while (pos)
	{
		if (Add(other.GetNext(pos)))
			nAdded++;
	}

	return nAdded;
}

BOOL CTDCAttributeMap::CanAdd(TDC_ATTRIBUTE nAttrib) const
{
	BOOL bCanAdd = FALSE;

	if (IsTaskAttribute(nAttrib))
	{
		// May not contain non-task attributes
		// which could only ever be the first attribute
		POSITION pos = GetStartPosition();

		if (pos)
			bCanAdd = IsTaskAttribute(GetNext(pos));
		else
			bCanAdd = TRUE;
	}
	else // rest of pseudo-attributes
	{
		// Must be empty
		bCanAdd = IsEmpty();
	}

	ASSERT(bCanAdd);
	return bCanAdd;
}

BOOL CTDCAttributeMap::IsTaskAttribute(TDC_ATTRIBUTE nAttrib)
{
	switch (nAttrib)
	{
	case TDCA_ALLOCBY:
	case TDCA_ALLOCTO:
	case TDCA_CATEGORY:
	case TDCA_COLOR:
	case TDCA_COMMENTS:
	case TDCA_COST:
	case TDCA_CREATEDBY:
	case TDCA_CREATIONDATE:
	case TDCA_DEPENDENCY:
	case TDCA_DONEDATE:
	case TDCA_DONETIME:
	case TDCA_DUEDATE:
	case TDCA_DUETIME:
	case TDCA_EXTERNALID:
	case TDCA_FILEREF:
	case TDCA_FLAG:
	case TDCA_HTMLCOMMENTS:
	case TDCA_ICON:
	case TDCA_ID:
	case TDCA_LOCK:
	case TDCA_LASTMODDATE:
	case TDCA_LASTMODBY:
	case TDCA_METADATA:
	case TDCA_PARENTID:
	case TDCA_PATH:
	case TDCA_PERCENT:
	case TDCA_POSITION:
	case TDCA_PRIORITY:
	case TDCA_RECURRENCE:
	case TDCA_RISK:
	case TDCA_STARTDATE:
	case TDCA_STARTTIME:
	case TDCA_STATUS:
	case TDCA_SUBTASKDONE:
	case TDCA_TAGS:
	case TDCA_TASKNAME:
	case TDCA_TIMEEST:
	case TDCA_TIMESPENT:
	case TDCA_VERSION:
		return TRUE;
	}

	// all else
	return CTDCCustomAttributeHelper::IsCustomAttribute(nAttrib);
}

void CTDCAttributeMap::Load(const IPreferences* pPrefs, LPCTSTR szKey, LPCTSTR szValueKeyFmt)
{
	CTDCBaseEnumSet<TDC_ATTRIBUTE>::Load(pPrefs, szKey, szValueKeyFmt, TDCA_NONE);
}

//////////////////////////////////////////////////////////////////////

CTDCAttributeArray::CTDCAttributeArray()
{
}

CTDCAttributeArray::CTDCAttributeArray(const CTDCAttributeArray& aOther)
{
	Copy(aOther);
}

CTDCAttributeArray::~CTDCAttributeArray()
{
}

BOOL CTDCAttributeArray::Has(TDC_ATTRIBUTE nAttrib) const
{
	return Misc::HasT(nAttrib, *this);
}

BOOL CTDCAttributeArray::AddUnique(TDC_ATTRIBUTE nAttrib)
{
	return Misc::AddUniqueItemT(nAttrib, *this);
}

void CTDCAttributeArray::Remove(TDC_ATTRIBUTE nAttrib)
{
	Misc::RemoveItemT(nAttrib, *this);
}

BOOL CTDCAttributeArray::MatchAll(const CTDCAttributeArray& aOther) const
{
	return Misc::MatchAllT(*this, aOther, FALSE);
}

//////////////////////////////////////////////////////////////////////

CTDCColumnIDMap::CTDCColumnIDMap()
	: CTDCBaseEnumSet<TDC_COLUMN>()
{
}

CTDCColumnIDMap::CTDCColumnIDMap(TDC_COLUMN nColID)
	: CTDCBaseEnumSet<TDC_COLUMN>()
{
	Add(nColID);
}

CTDCColumnIDMap::CTDCColumnIDMap(const CTDCColumnIDMap& mapOther)
	: CTDCBaseEnumSet<TDC_COLUMN>(mapOther)
{
}

CTDCColumnIDMap::~CTDCColumnIDMap()
{
}


void CTDCColumnIDMap::Load(const IPreferences* pPrefs, LPCTSTR szKey, LPCTSTR szValueKeyFmt)
{
	CTDCBaseEnumSet<TDC_COLUMN>::Load(pPrefs, szKey, szValueKeyFmt, TDCC_NONE);
}

BOOL CTDCColumnIDMap::Has(TDC_COLUMN nColID) const
{
	if (CTDCBaseEnumSet<TDC_COLUMN>::Has(nColID))
		return TRUE;

	// Date/Time fallbacks
	switch (nColID)
	{
	case TDCC_CREATIONDATE:	return CTDCBaseEnumSet<TDC_COLUMN>::Has(TDCC_CREATIONTIME);
	case TDCC_STARTDATE:	return CTDCBaseEnumSet<TDC_COLUMN>::Has(TDCC_STARTTIME);
	case TDCC_DUEDATE:		return CTDCBaseEnumSet<TDC_COLUMN>::Has(TDCC_DUETIME);
	case TDCC_DONEDATE:		return CTDCBaseEnumSet<TDC_COLUMN>::Has(TDCC_DONETIME);
	}

	return FALSE;
}

TDC_COLUMN CTDCColumnIDMap::GetFirst() const
{
	if (GetCount())
	{
		POSITION pos = GetStartPosition();
		return GetNext(pos);
	}

	// else
	return TDCC_NONE;
}

//////////////////////////////////////////////////////////////////////

CTDCColumnIDArray::CTDCColumnIDArray()
{
}

CTDCColumnIDArray::CTDCColumnIDArray(const CTDCColumnIDArray& aOther)
{
	Copy(aOther);
}

CTDCColumnIDArray::~CTDCColumnIDArray()
{
}

BOOL CTDCColumnIDArray::Has(TDC_COLUMN nColID) const
{
	return Misc::HasT(nColID, *this);
}

BOOL CTDCColumnIDArray::AddUnique(TDC_COLUMN nColID)
{
	return Misc::AddUniqueItemT(nColID, *this);
}

void CTDCColumnIDArray::Remove(TDC_COLUMN nColID)
{
	Misc::RemoveItemT(nColID, *this);
}

BOOL CTDCColumnIDArray::MatchAll(const CTDCColumnIDArray& aOther) const
{
	return Misc::MatchAllT(*this, aOther, FALSE);
}

//////////////////////////////////////////////////////////////////////
